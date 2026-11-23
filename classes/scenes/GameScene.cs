using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Entities.CatchableItems;
using EXILION.Entities;
using EXILION.Items;
using EXILION.UI;
using EXILION.World;
using EXILION.UI.HUD;
using EXILION.Saving;
using EXILION.Logic;
using EXILION.Client;
using EXILION.Network;
using System;
using System.Linq;

namespace EXILION.Scenes;

public class GameScene : Scene
{

    // Game stats

    private float nightOpacity = 0f;
    private const int DIURNAL_PRESET_TIME = 200; // in seconds
    private int diurnalCycleTime = DIURNAL_PRESET_TIME; // in seconds
    private int dayCount = 1;
    private readonly GameClock gameClock = new GameClock();
    private const float MAX_NIGHT_OPACITY = 0.65f;
    private const float NIGHT_TRANSITION_SPEED = 0.15f;
    private bool night = false;
    private Color nightColor = new Color(30, 10, 45);

    // Panels

    private PausePanel pausePanel;
    private DeathPanel deathPanel;
    private VictoryPanel victoryPanel;

    // Player
    private Player player;
    private List<Creature> creatures = new List<Creature>();
    private CreatureSpawnGenerator creatureSpawnGenerator;
    private readonly GameWorldLogic gameLogic = new GameWorldLogic();
    private Texture2D pixel;
    private HUD HUD;
    private InputManager input;
    private InteractionPrompt interactionPrompt;

    // Songs queue
    private List<Song> songsQueue = new List<Song>
    {
        Assets.Songs.exiliated,
        Assets.Songs.drownInInterrogations
    };

    private int currentSongIndex = 0;

    private List<CatchableItem> catchableItems;
    private World.World world;
    private MapRenderer mapRenderer;
    private EntityRenderer entityRenderer;


    private Camera camera;
    private GraphicsDevice graphicsDevice;

    // World Saving
    private WorldSaveData saveData;
    private WorldSaveService saveService = new WorldSaveService();
    private string path;
    private readonly GameConnection? networkConnection;
    private readonly bool isOnline;
    private bool isNewGame;
    private bool isLoaded = false;
    private bool blockPlacementMode;
    private int Seed;
    private readonly Dictionary<Guid, RemotePlayer> remotePlayers = new();
    private readonly HashSet<int> seenWorldHitEvents = new();
    private readonly Queue<int> worldHitEventOrder = new();
    private PlayerSnapshot? authoritativePlayerSnapshot;
    private float localDamageFeedbackRemaining;
    private bool respawnPending;
    private float respawnPendingRemaining;
    private bool restoredOnlineProfile;
    private Vector2 localPredictedPosition;

    public GameScene(IGameData gameData, string path) : base(gameData)
    {
        this.input = gameData.GetInput();
        this.camera = gameData.GetCamera();
        this.graphicsDevice = gameData.GetGraphicsDevice();
        this.path = path;

        saveData = saveService.LoadAsync(path).Result;
        isNewGame = saveData.IsNewWorld;

        Music.Play(songsQueue[currentSongIndex], 1f);
    }

    /// <summary>Creates a client view from the state received during the multiplayer handshake.</summary>
    public GameScene(IGameData gameData, WorldSaveData networkWorld, GameConnection connection) : base(gameData)
    {
        input = gameData.GetInput();
        camera = gameData.GetCamera();
        graphicsDevice = gameData.GetGraphicsDevice();
        saveData = networkWorld;
        // The local player owns HUD/input state; online movement and world state come from the server.
        saveData.IsNewWorld = true;
        isNewGame = true;
        networkConnection = connection;
        isOnline = true;
        Music.Play(songsQueue[currentSongIndex], 1f);
    }

    public override void LoadContent()
    {
        // Init Seed
        Seed = saveData.WorldSeed;
        if (isOnline)
        {
            diurnalCycleTime = saveData.WorldTime;
            dayCount = Math.Max(1, saveData.DayCount);
            night = saveData.IsNight;
            nightOpacity = night ? MAX_NIGHT_OPACITY : 0f;
            gameClock.Restore(diurnalCycleTime, dayCount, night);
        }

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        pausePanel = new PausePanel(GameData, isOnline);
        deathPanel = new DeathPanel(GameData);
        victoryPanel = new VictoryPanel(GameData);

        deathPanel.Respawn += OnRespawnRequested;
        victoryPanel.ContinuePlaying += resumeAfterVictory;
        Music.musicStop += changeMusic;

        catchableItems = new List<CatchableItem>();

        int tileSize = (int)GameContext.ScaleXY(64);
        entityRenderer = new EntityRenderer(GameContext.ScaleXY(1));
        world = new World.World(seed: Seed, tileSize: tileSize) { RenderDistanceChunks = 2 };
        creatureSpawnGenerator = new CreatureSpawnGenerator(world);
        world.ObjectDestroyed += dropWorldObjectItems;
        world.ObjectDamaged += OnWorldObjectDamaged;

        interactionPrompt = new InteractionPrompt(Assets.Fonts.PixelArt, GameContext.ScaleXY(1));

        if (isNewGame)
        {
            startNewGame();
            saveGame();
        }
        else
        {
            loadGame();
        }

        world.UpdateAroundPosition(player.position);
        mapRenderer = new MapRenderer(Assets.Sprites.Tileset, Assets.Sprites.Objects, tileSize);

        isLoaded = true;
    }

    private void dropWorldObjectItems(WorldObject worldObject)
    {
        Vector2 dropPosition = new Vector2(worldObject.TileX * world.TileSize, worldObject.TileY * world.TileSize);
        foreach (var drop in worldObject.Definition.Drops)
        {
            if (drop.Quantity <= 0) continue;
            catchableItems.Add(
                new CatchableItem(
                    new ItemStack(drop.Item, drop.Quantity),
                    dropPosition,
                    DropHitboxSize()
                )
            );
        }
    }

    private void OnWorldObjectDamaged(WorldObjectId id, ObjectType type)
    {
        ObjectAudio.PlayHit(type);
        mapRenderer?.PlayObjectHitAnimation(id);
    }

    private void dropCreatureItems(Creature creature)
    {
        IReadOnlyList<ItemStack>? dropItems = creature.GetDropItems();

        if(dropItems == null) return;

        foreach (ItemStack drop in dropItems)
        {
            if (drop == null || drop.Quantity <= 0) continue;
            catchableItems.Add(new CatchableItem(
                drop,
                creature.position,
                DropHitboxSize()
            ));
        }
    }

    private void loadGame()
    {

        world.RestoreObjectStates(saveData.WorldObjectStates.Select(state => state.ToState()));

        diurnalCycleTime = saveData.WorldTime;
        dayCount = Math.Max(1, saveData.DayCount);

        night = saveData.IsNight;
        gameClock.Restore(diurnalCycleTime, dayCount, night);
        if (night) nightOpacity = MAX_NIGHT_OPACITY;
        
        Seed = saveData.WorldSeed;
        processPlayerSpawn(false);

        creatures.Clear();
        foreach (CreatureSaveData creatureData in saveData.Creatures ?? new List<CreatureSaveData>())
        {
            Creature creature = CreatureFactory.CreateRestoredCreature(creatureData, EntityHitboxSize());
            creature.SoundRequested += GameAudio.Play;
            creatures.Add(creature);
        }
        
        foreach(var catchableItem in saveData.CatchableItems)
        {

            var item = ItemRegistry.GetById(catchableItem.ItemId);
            if (item == null) continue;

            catchableItems.Add(

                new CatchableItem(
                    new ItemStack(item, catchableItem.Quantity),
                    new Vector2(catchableItem.PositionX, catchableItem.PositionY),
                    DropHitboxSize()
                )

            );
        }
    }

    public void saveGame()
    {
        if (isOnline) return;

        saveData.IsNewWorld = false;
        saveData.WorldTime = diurnalCycleTime;
        saveData.IsNight = night;
        saveData.DayCount = dayCount;

        saveData.Player = player.GetSaveData();
        saveData.CatchableItems.Clear();
        foreach (var catchableItem in catchableItems)
        {
            saveData.CatchableItems.Add(catchableItem.GetSaveData());
        }

        saveData.WorldObjectStates.Clear();
        saveData.WorldObjectStates.AddRange(world.CaptureObjectStates().Select(WorldObjectStateSaveData.FromState));

        saveData.Creatures.Clear();
        saveData.Creatures.AddRange(creatures
            .Where(creature => !creature.isDead && !creature.ShouldDespawn)
            .Select(creature => creature.GetSaveData()));

        saveService.SaveAsync(path, saveData).Wait();
    }

    private void startNewGame()
    {
        processPlayerSpawn(true);

        catchableItems.Add(

            new CatchableItem(
                new ItemStack(ItemRegistry.GeneradorO2, 1),
                new Vector2(0, 0),
                DropHitboxSize()
            )

        );

    }

    public override void Update(GameTime gameTime)
    {
        if(stopUpdating || !isLoaded) return;

        if (isOnline && networkConnection?.IsDisconnected == true)
        {
            SceneManager.ChangeScene(new MainMenu(GameData));
            return;
        }

        if (isOnline && networkConnection != null)
        {
            while (networkConnection.Snapshots.TryDequeue(out var snapshot))
            {
                foreach (var remote in snapshot.Players)
                {
                    if (remote.PlayerId == networkConnection.Ticket.SessionId)
                    {
                        if (remote.IsDead)
                        {
                            if (respawnPending && respawnPendingRemaining > 0f)
                                continue;
                            respawnPending = false;
                            authoritativePlayerSnapshot = null;
                            localPredictedPosition = Vector2.Zero;
                            if (player != null) processPlayerDeath();
                            continue;
                        }
                        respawnPending = false;
                        respawnPendingRemaining = 0f;
                        int previousHealth = authoritativePlayerSnapshot?.Health ?? player?.health ?? remote.Health;
                        if (remote.Health < previousHealth)
                        {
                            localDamageFeedbackRemaining = 0.3f;
                            camera.damageShake(previousHealth - remote.Health);
                            GameAudio.Play(GameAudioCue.PlayerDamage);
                        }
                        if (player != null)
                        {
                            Vector2 serverPosition = new(remote.X, remote.Y);
                            localPredictedPosition = authoritativePlayerSnapshot != null
                                ? Vector2.Lerp(localPredictedPosition, serverPosition, 0.35f)
                                : serverPosition;
                            player.position = serverPosition;
                            player.ApplyNetworkStats(remote.Health, remote.Hunger, remote.Thirst, remote.Oxygen);
                        }
                        authoritativePlayerSnapshot = remote;
                        continue;
                    }

                    if (remote.IsDead)
                    {
                        remotePlayers.Remove(remote.PlayerId);
                        continue;
                    }

                    if (!remotePlayers.TryGetValue(remote.PlayerId, out var remotePlayer))
                    {
                        remotePlayer = new RemotePlayer(remote.PlayerId);
                        remotePlayers.Add(remote.PlayerId, remotePlayer);
                    }

                    remotePlayer.ApplySnapshot(remote);
                }
                world?.ApplyObjectStates(snapshot.WorldObjects);
                ApplyCreatureSnapshots(snapshot.Creatures);
                ApplyCatchableItemSnapshots(snapshot.CatchableItems);
                foreach (WorldObjectHitEvent hitEvent in snapshot.WorldHitEvents)
                {
                    if (!seenWorldHitEvents.Add(hitEvent.EventId)) continue;
                    worldHitEventOrder.Enqueue(hitEvent.EventId);
                    while (worldHitEventOrder.Count > 2048)
                        seenWorldHitEvents.Remove(worldHitEventOrder.Dequeue());
                    ObjectAudio.PlayHit(hitEvent.Type);
                    mapRenderer?.PlayObjectHitAnimation(hitEvent.ObjectId);
                }
                diurnalCycleTime = snapshot.WorldTime;
                dayCount = snapshot.DayCount;
                night = snapshot.IsNight;
                HUD?.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);
                HUD?.setDayCount(dayCount);
            }
        }

        mapRenderer?.Update(gameTime);
        UpdateCatchableItemAnimations(gameTime);
        if (respawnPending)
            respawnPendingRemaining = Math.Max(0f, respawnPendingRemaining - (float)gameTime.ElapsedGameTime.TotalSeconds);

        MouseState mouse = input.CurrentMouse;

        if(deathPanel.enabled)
        {
            deathPanel.Update();
            return;
        }

        if (victoryPanel.Enabled)
        {
            victoryPanel.Update(gameTime);
            return;
        }

        if (input.IsKeyPressed(Keys.Escape))
        {

            if (HUD.inventoryUI.expanded)
            {
                HUD.inventoryUI.Close();
            } else
            {
                pausePanel.enabled = !pausePanel.enabled;
            }

        }
        if (input.IsKeyPressed(Keys.F1) && player != null)
        {
            HUD.toggle();
        }
        if (input.IsKeyPressed(Keys.G))
        {
            saveGame();
        }
        if (input.IsKeyPressed(Keys.B))
        {
            blockPlacementMode = !blockPlacementMode;
        }

        if (pausePanel.enabled)
        {
            pausePanel.Update();
            return;
        } 
            
        
        if(player != null)
        {
            world?.UpdateObjects((float)gameTime.ElapsedGameTime.TotalSeconds);
            Vector2 mouseWorldPosition = mouse.Position.ToVector2();

            if (camera != null)
            {
                Matrix inverseCamera = Matrix.Invert(camera.GetViewMatrix());
                mouseWorldPosition = Vector2.Transform(mouseWorldPosition, inverseCamera);
            }

            if (!isOnline) UpdateDiurnalCycle(gameTime);
            UpdateNightTransition(gameTime);

            HUD.Update(gameTime);
            if (input.IsKeyPressed(Keys.H))
            {
                GameContext.showHitboxes = !GameContext.showHitboxes;
            }

            bool placeBlock = blockPlacementMode && input.IsLeftMousePressed() && !input.IsMouseCaptured;
            bool wasMouseCaptured = input.IsMouseCaptured;
            if (placeBlock) input.IsMouseCaptured = true;
            PlayerInput playerInput = new PlayerInput(
                MoveUp: input.IsKeyHeld(Keys.W),
                MoveDown: input.IsKeyHeld(Keys.S),
                MoveLeft: input.IsKeyHeld(Keys.A),
                MoveRight: input.IsKeyHeld(Keys.D),
                Run: input.IsKeyHeld(Keys.LeftShift),
                PrimaryActionPressed: input.IsLeftMousePressed(),
                PrimaryActionCaptured: input.IsMouseCaptured);
            if (isOnline)
                networkConnection?.SetInput(
                    playerInput with { InteractPressed = input.IsKeyPressed(Keys.E) },
                    new System.Numerics.Vector2(mouseWorldPosition.X, mouseWorldPosition.Y),
                    player.rightHand.HeldItem?.Id ?? -1,
                    placeBlock);
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (isOnline)
                localPredictedPosition = player.PredictPosition(localPredictedPosition, playerInput, deltaTime, world);
            player.Update(mouseWorldPosition, playerInput, deltaTime, world,
                allowMovement: !isOnline, simulateStats: !isOnline);

            if (!isOnline)
            {
                foreach (Creature creature in creatures) creature.Update(deltaTime, world, player.position, night);
                gameLogic.ProcessCreatureContactDamage(player, creatures, deltaTime);
            }

            input.IsMouseCaptured = wasMouseCaptured;

            if (placeBlock) gameLogic.TryPlaceStructure(player, world, mouseWorldPosition);

            if (!isOnline)
            {
                gameLogic.ResolvePlayerAttacks(player, world, creatures);
                foreach (Creature creature in creatures.Where(creature => creature.isDead))
                    dropCreatureItems(creature);
                creatures.RemoveAll(creature => creature.isDead);
                creatures.RemoveAll(creature => creature.ShouldDespawn);
            }

            // Cierre automático de la mesa si el jugador se aleja
            if (HUD.craftingUI.AtCraftingTable && !world.IsNearCraftingTable(player.position))
            {
                HUD.CloseCraftingTable();
            }
            UpdateInteractionPrompt();

            if (input.IsKeyPressed(Keys.E))
            {
                if (HUD.craftingUI.AtCraftingTable)
                {
                    HUD.CloseCraftingTable();
                }
                else
                {
                    if (world.IsNearExit(player.position))
                    {
                        victoryPanel.Enabled = true;
                        Music.musicStop -= changeMusic;
                        Music.Play(Assets.Songs.MenuMusic);
                    }
                    else
                    {
                        bool touchedItem = gameLogic.TryPickupItems(player, catchableItems);

                        if (!touchedItem && world.IsNearCraftingTable(player.position))
                        {
                            HUD.OpenCraftingTable();
                        }
                    }
                }
            }
        
            world.UpdateAroundPosition(player.position);
            if (!isOnline) world.UpdateRegrowth((float)gameTime.ElapsedGameTime.TotalSeconds, player.position);
            Creature spawnedCreature = isOnline ? null : creatureSpawnGenerator.TrySpawn(
                (float)gameTime.ElapsedGameTime.TotalSeconds,
                player.position,
                night,
                dayCount,
                creatures,
                GameContext.ScaleX(20),
                (type, position) => CreatureFactory.CreateCreature(type, position, EntityHitboxSize()));
            if (spawnedCreature != null)
            {
                spawnedCreature.SoundRequested += GameAudio.Play;
                creatures.Add(spawnedCreature);
            }
            camera.Follow(isOnline ? localPredictedPosition : player.position);

            if (player.isDead)
            {
                processPlayerDeath();
            }

        }
        

    }

    private void ApplyCreatureSnapshots(CreatureSnapshot[] snapshots)
    {
        Dictionary<Guid, Creature> existing = creatures.ToDictionary(creature => creature.NetworkId);
        List<Creature> synchronized = new(snapshots.Length);
        foreach (CreatureSnapshot snapshot in snapshots)
        {
            bool isNewCreature = false;
            if (!existing.TryGetValue(snapshot.CreatureId, out Creature creature))
            {
                isNewCreature = true;
                creature = CreatureFactory.CreateCreature(snapshot.Type,
                    new Vector2(snapshot.X, snapshot.Y), EntityHitboxSize(), snapshot.CreatureId);
                creature.SoundRequested += GameAudio.Play;
            }
            creature.ApplyNetworkState(snapshot.X, snapshot.Y, snapshot.Rotation, snapshot.Health, !isNewCreature);
            synchronized.Add(creature);
        }
        creatures.Clear();
        creatures.AddRange(synchronized);
    }

    private void UpdateCatchableItemAnimations(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (CatchableItem item in catchableItems)
        {
            if (!isOnline) item.Update(deltaTime);
            else item.UpdateNetworkVisual(deltaTime);
            entityRenderer.UpdateDrop(item, deltaTime);
        }
        catchableItems.RemoveAll(item => item.Picked);
        entityRenderer.PruneDropAnimations(catchableItems);
        localDamageFeedbackRemaining = Math.Max(0f, localDamageFeedbackRemaining - deltaTime);
        foreach (RemotePlayer remotePlayer in remotePlayers.Values)
            remotePlayer.Update(deltaTime);
        if (isOnline)
            foreach (Creature creature in creatures)
            {
                creature.UpdateNetworkVisual(deltaTime);
                creature.Update(deltaTime);
            }
    }

    private void ApplyCatchableItemSnapshots(CatchableItemSnapshot[] snapshots)
    {
        Dictionary<Guid, CatchableItem> existing = catchableItems.ToDictionary(item => item.NetworkId);
        List<CatchableItem> synchronized = new(snapshots.Length);
        foreach (CatchableItemSnapshot snapshot in snapshots)
        {
            Item? type = ItemRegistry.GetById(snapshot.ItemTypeId);
            if (type == null || snapshot.Quantity <= 0) continue;
            if (existing.TryGetValue(snapshot.ItemId, out CatchableItem? item)
                && item.Stack.Item.Id == snapshot.ItemTypeId
                && item.Stack.Quantity == snapshot.Quantity)
            {
                item.ApplyNetworkPosition(new Vector2(snapshot.X, snapshot.Y));
                synchronized.Add(item);
                continue;
            }
            synchronized.Add(new CatchableItem(new ItemStack(type, snapshot.Quantity),
                new Vector2(snapshot.X, snapshot.Y), DropHitboxSize(), snapshot.ItemId));
        }
        catchableItems.Clear();
        catchableItems.AddRange(synchronized);
    }

    private void UpdateInteractionPrompt()
    {
        if (player == null || HUD.craftingUI.AtCraftingTable)
        {
            interactionPrompt.Visible = false;
            return;
        }

        Rectangle playerHitbox = player.GetHitbox();

        bool nearItem = catchableItems.Any(item =>
            playerHitbox.Intersects(item.GetHitbox())
        );

        bool nearCraftingTable = world.IsNearCraftingTable(player.position);
        bool nearExit = world.IsNearExit(player.position);

        interactionPrompt.Visible = nearItem || nearCraftingTable || nearExit;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        float playerY = player != null ? player.position.Y : float.MaxValue;

        mapRenderer?.Draw(spriteBatch, world);
        mapRenderer?.DrawOxygenRanges(spriteBatch, world, pixel);

        // Entities
        if(catchableItems != null)
        {
            foreach (var item in catchableItems)
            {
                entityRenderer.DrawDrop(spriteBatch, item, pixel, GameContext.showHitboxes);
            }
        }
        
        
        // Player & objects
        mapRenderer?.DrawObjects(spriteBatch, world, false);
        if (isOnline && player != null)
            entityRenderer.DrawPlayer(spriteBatch, player, pixel, GameContext.showHitboxes,
                localPredictedPosition - player.position, localDamageFeedbackRemaining);
        else if (player != null)
            entityRenderer.DrawPlayer(spriteBatch, player, pixel, GameContext.showHitboxes);
        foreach (RemotePlayer remotePlayer in remotePlayers.Values)
            entityRenderer.DrawRemotePlayer(spriteBatch, remotePlayer);
        HUD?.DrawStatAdvice(spriteBatch);

        foreach (Creature creature in creatures)
            entityRenderer.DrawCreature(spriteBatch, creature, pixel, GameContext.showHitboxes);

        mapRenderer?.DrawObjects(spriteBatch, world, true);

        // Interaction Prompt
        if (interactionPrompt != null && !pausePanel.enabled && !deathPanel.enabled && !victoryPanel.Enabled && player != null)
        {
            interactionPrompt.Draw(spriteBatch, pixel, player.GetHitbox());
        }

        if (GameContext?.showHitboxes == true) mapRenderer?.DrawObjectHitboxes(spriteBatch, world, pixel);
        
    }

    public override void DrawUI(SpriteBatch spriteBatch)
    {

        if(!isLoaded) return;
        // Night Filter
        if (nightOpacity > 0) spriteBatch.Draw(pixel, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), nightColor * nightOpacity);

        // UI
        HUD?.Draw(spriteBatch);

        // Panels
        pausePanel.Draw(spriteBatch);
        deathPanel.Draw(spriteBatch);
        victoryPanel.Draw(spriteBatch, pixel);
    }

    public override void UnloadContent()
    {
        stopUpdating = true;

        // Events
        Music.musicStop -= changeMusic;
        if (world != null) world.ObjectDamaged -= OnWorldObjectDamaged;
        deathPanel.Respawn -= OnRespawnRequested;
        victoryPanel.ContinuePlaying -= resumeAfterVictory;
        if (player != null)
        {
            player.Inventory.UpdateInventoryUI -= HUD.inventoryUI.UpdateHandItem;
            player.DamageTaken -= camera.damageShake;
        } 

        HUD.inventoryUI.ItemDropped -= DropItem;
        // UI
        HUD = null;
        pausePanel = null;
        deathPanel = null;
        victoryPanel = null;
        interactionPrompt = null;

        // Entities
        catchableItems?.Clear();
        catchableItems = null;
        creatures?.Clear();

        // World
        world = null;
        mapRenderer = null;

        // Player
        player = null;

        // Utils
        pixel?.Dispose();
        pixel = null;
        GameContext = null;
        camera = null;
        input = null;

        // Music queue
        songsQueue.Clear();
        networkConnection?.Dispose();

    }

    private void UpdateDiurnalCycle(GameTime gameTime)
    {
        bool phaseChanged = gameClock.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        diurnalCycleTime = gameClock.RemainingSeconds;
        dayCount = gameClock.DayCount;
        night = gameClock.IsNight;
        if (phaseChanged) HUD.setDayCount(dayCount);
        HUD.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);
    }

    private void processPlayerDeath()
    {
        HUD.hide();

        // Drop inventory items
        Inventory inventory = player.Inventory;

        for(int i = 0; i < inventory.Capacity; i++)
        {
            ItemStack itemStack = inventory.GetSlot(i);
            if(itemStack == null) continue;

            if (isOnline)
                networkConnection?.QueueDropItem(itemStack.Item.Id, itemStack.Quantity);
            else
                catchableItems.Add(new CatchableItem(itemStack, player.position, DropHitboxSize()));
        }

        ItemStack? equippedArmor = inventory.TakeEquippedArmor();
        if (equippedArmor != null)
        {
            if (isOnline)
                networkConnection?.QueueDropItem(equippedArmor.Item.Id, equippedArmor.Quantity);
            else
                catchableItems.Add(new CatchableItem(equippedArmor, player.position, DropHitboxSize()));
        }

        player.DamageTaken -= camera.damageShake;
        player.Inventory.UpdateInventoryUI -= HUD.inventoryUI.UpdateHandItem;

        HUD = null;
        player = null;
        localPredictedPosition = Vector2.Zero;
        deathPanel.enabled = true;
    }

    private void processPlayerSpawn(bool isNewGame)
    {
        player = new Player(Vector2.Zero, EntityHitboxSize());
        player.SoundRequested += GameAudio.Play;
        if (isOnline && !restoredOnlineProfile && networkConnection != null)
        {
            player.Restore(networkConnection.Ticket.Player);
            restoredOnlineProfile = true;
        }
        else if (!isNewGame)
        {
            player.Restore(saveData.Player);
        }

        HUD = new HUD(player, GameContext, graphicsDevice, input, DIURNAL_PRESET_TIME);
        HUD.setDayCount(dayCount);

         HUD.inventoryUI.ItemDropped += DropItem;

        // update stats from beggining
        HUD.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);
        player.ForceUpdateStats();

        player.DamageTaken += camera.damageShake;
        player.Inventory.UpdateInventoryUI += HUD.inventoryUI.UpdateHandItem;

        if (isNewGame)
        {
            player.position = world.FindSpawnPosition();
        }
        else
        {
            world.UpdateAroundPosition(player.position);
        }
        localPredictedPosition = player.position;
    }

    private void OnRespawnRequested(bool isNewGame)
    {
        if (isOnline)
        {
            respawnPending = true;
            respawnPendingRemaining = 5f;
            authoritativePlayerSnapshot = null;
            localDamageFeedbackRemaining = 0f;
            networkConnection?.QueueRespawnRequest();
        }
        processPlayerSpawn(isNewGame);
    }

    private void DropItem(ItemStack stack)
    {
        if (stack == null || stack.Quantity <= 0) return;
        if (isOnline)
        {
            networkConnection?.QueueDropItem(stack.Item.Id, stack.Quantity);
            return;
        }

        Rectangle playerHitbox = player.GetHitbox();
        Vector2 dropPosition = new(
            player.position.X,
            playerHitbox.Bottom + GameContext.ScaleY(16));

        catchableItems.Add(
            new CatchableItem(
                stack,
                dropPosition,
                DropHitboxSize()
            )
        );
    }

    private Point EntityHitboxSize() => new(GameContext.ScaleX(40), GameContext.ScaleY(40));

    private Point DropHitboxSize() => new(GameContext.ScaleX(28), GameContext.ScaleY(28));

    private void UpdateNightTransition(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (night)
        {
            nightOpacity += NIGHT_TRANSITION_SPEED * deltaTime;
            if (nightOpacity > MAX_NIGHT_OPACITY) nightOpacity = MAX_NIGHT_OPACITY;
        }
        else
        {
            nightOpacity -= NIGHT_TRANSITION_SPEED * deltaTime;
            if (nightOpacity < 0) nightOpacity = 0;
        }
    }

    private void changeMusic()
    {
        currentSongIndex++;
        if (currentSongIndex >= songsQueue.Count) currentSongIndex = 0;
        Music.Play(songsQueue[currentSongIndex], 3f);
    }

    private void resumeAfterVictory()
    {
        Music.musicStop += changeMusic;
        Music.Play(songsQueue[currentSongIndex], 1f);
    }

}
