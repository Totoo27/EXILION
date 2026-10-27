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
using System;
using System.Linq;

namespace EXILION.Scenes;

public class GameScene : Scene
{

    // Game stats

    private float timer = 0f;
    private const float TIMER_INTERVAL = 1f;
    private float nightOpacity = 0f;
    private const int DIURNAL_PRESET_TIME = 200; // in seconds
    private int diurnalCycleTime = DIURNAL_PRESET_TIME; // in seconds
    private const float MAX_NIGHT_OPACITY = 0.65f;
    private const float NIGHT_TRANSITION_SPEED = 0.15f;
    private bool night = false;
    private Color nightColor = new Color(15, 10, 35);

    // Panels

    private PausePanel pausePanel;
    private DeathPanel deathPanel;


    // Player
    private Player player;
    private Texture2D pixel;
    private HUD HUD;
    private InputManager input;

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


    private Camera camera;
    private GraphicsDevice graphicsDevice;

    // World Saving
    private WorldSaveData saveData;
    private WorldSaveService saveService = new WorldSaveService();
    private string path;
    private bool isNewGame;
    private bool isLoaded = false;
    private bool blockPlacementMode;
    private int Seed;

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

    public override void LoadContent()
    {
        // Init Seed
        Seed = saveData.WorldSeed;

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        pausePanel = new PausePanel(GameData);
        deathPanel = new DeathPanel(GameData);

        deathPanel.Respawn += processPlayerSpawn;
        Music.musicStop += changeMusic;

        catchableItems = new List<CatchableItem>();

        int tileSize = (int)GameContext.ScaleXY(64);
        world = new World.World(seed: Seed, tileSize: tileSize) { RenderDistanceChunks = 2 };
        world.ObjectDestroyed += dropWorldObjectItems;

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
                    new Sprite(drop.Item.Icon, GameContext.ScaleXY(1)),
                    GameContext
                )
            );
        }
    }

    private void loadGame()
    {

        world.RestoreObjectStates(saveData.WorldObjectStates.Select(state => state.ToState()));

        diurnalCycleTime = saveData.WorldTime;
        night = saveData.IsNight;
        if (night)
        {
            nightOpacity = 1f;
        }
        Seed = saveData.WorldSeed;
        processPlayerSpawn(false);
        
        foreach(var catchableItem in saveData.CatchableItems)
        {

            var item = ItemRegistry.GetById(catchableItem.ItemId);
            if (item == null) continue;

            catchableItems.Add(

                new CatchableItem(
                    new ItemStack(item, catchableItem.Quantity),
                    new Vector2(catchableItem.PositionX, catchableItem.PositionY),
                    new Sprite(item.Icon, GameContext.ScaleXY(1)),
                    GameContext
                )

            );
        }
    }

    public void saveGame()
    {

        saveData.IsNewWorld = false;
        saveData.WorldTime = diurnalCycleTime;
        saveData.IsNight = night;

        saveData.Player = player.GetSaveData();
        saveData.CatchableItems.Clear();
        foreach (var catchableItem in catchableItems)
        {
            saveData.CatchableItems.Add(catchableItem.GetSaveData());
        }

        saveData.WorldObjectStates.Clear();
        saveData.WorldObjectStates.AddRange(
            world.CaptureObjectStates().Select(WorldObjectStateSaveData.FromState));

        saveService.SaveAsync(path, saveData).Wait();
    }

    private void startNewGame()
    {
        processPlayerSpawn(true);
        
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.Madera, 64),
                new Vector2(100, 100),
                new Sprite(ItemRegistry.Madera.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.Piedra, 3),
                new Vector2(125, 100),
                new Sprite(ItemRegistry.Piedra.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.AguaPurificada, 2),
                new Vector2(300, 100),
                new Sprite(ItemRegistry.AguaPurificada.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.CarneCocinada, 2),
                new Vector2(400, 100),
                new Sprite(ItemRegistry.CarneCocinada.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.OxigenoEmbotellado, 2),
                new Vector2(500, 100),
                new Sprite(ItemRegistry.OxigenoEmbotellado.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.HachaTest, 1),
                new Vector2(600, 100),
                new Sprite(ItemRegistry.HachaTest.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
    }

    public override void Update(GameTime gameTime)
    {
        if(stopUpdating || !isLoaded) return;

        MouseState mouse = input.CurrentMouse;

        if(deathPanel.enabled)
        {
            deathPanel.Update();
            return;
        }

        if (input.IsKeyPressed(Keys.Escape))
        {
            pausePanel.enabled = true;
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
            Vector2 mouseWorldPosition = mouse.Position.ToVector2();

            if (camera != null)
            {
                Matrix inverseCamera = Matrix.Invert(camera.GetViewMatrix());
                mouseWorldPosition = Vector2.Transform(mouseWorldPosition, inverseCamera);
            }

            UpdateDiurnalCycle(gameTime);
            UpdateNightTransition(gameTime);

            HUD.Update();
            bool placeBlock = blockPlacementMode && input.IsLeftMousePressed() && !input.IsMouseCaptured;
            bool wasMouseCaptured = input.IsMouseCaptured;
            if (placeBlock) input.IsMouseCaptured = true;
            player.Update(mouseWorldPosition, input, gameTime, world);
            input.IsMouseCaptured = wasMouseCaptured;
            if (placeBlock)
                world.TryPlaceBlock(mouseWorldPosition, player.position, player.GetHitbox().Width / 2f);
            ApplyPlayerAttackHits();

            if (input.IsKeyPressed(Keys.E))
            {
                Rectangle playerHitbox = player.GetHitbox();

                for (int i = catchableItems.Count - 1; i >= 0; i--)
                {
                    CatchableItem item = catchableItems[i];

                    if (playerHitbox.Intersects(item.GetHitbox()))
                    {
                        player.TryPickup(item);
                        if (item.Picked) catchableItems.RemoveAt(i);
                    }
                }
            }
        
            world.UpdateAroundPosition(player.position);
            camera.Follow(player.position);

            if (player.isDead)
            {
                processPlayerDeath();
            }

        }

    }

    private void ApplyPlayerAttackHits()
    {
        ApplyHandAttackHits(player.rightHand);
        ApplyHandAttackHits(player.leftHand);
    }

    private void ApplyHandAttackHits(Hand hand)
    {
        if (hand.CurrentHitbox is not AttackHitbox hitbox) return;
        world.DamageObjects(hitbox, hand.AttackDamage, hand.HeldItem, hand.RegisterHit);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        float playerY = player != null ? player.position.Y : float.MaxValue;

        mapRenderer?.Draw(spriteBatch, world);

        // Entities
        if(catchableItems != null)
        {
            foreach (var item in catchableItems)
            {
                item.Draw(spriteBatch, pixel);
            }
        }
        
        
        // Player & objects
        mapRenderer?.DrawObjects(spriteBatch, world, playerY, false);
        player?.Draw(spriteBatch, pixel);
        mapRenderer?.DrawObjects(spriteBatch, world, playerY, true);
        if (GameContext?.showHitboxes == true)
            mapRenderer?.DrawObjectHitboxes(spriteBatch, world, pixel);
        
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
    }

    public override void UnloadContent()
    {
        stopUpdating = true;

        // Events
        Music.musicStop -= changeMusic;
        deathPanel.Respawn -= processPlayerSpawn;
        if (player != null)
        {
            player.Inventory.UpdateInventoryUI -= HUD.inventoryUI.UpdateHandItem;
            player.HealthChanged -= camera.damageShake;
        } 

        // UI
        HUD = null;
        pausePanel = null;
        deathPanel = null;

        // Entities
        catchableItems?.Clear();
        catchableItems = null;

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

    }

    private void UpdateDiurnalCycle(GameTime gameTime)
    {
        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (timer >= TIMER_INTERVAL)
        {
            timer = 0f;
            diurnalCycleTime--;

            if (diurnalCycleTime <= 0)
            {

                diurnalCycleTime = DIURNAL_PRESET_TIME;
                night = !night;

            }

            HUD.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);

        }

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

            catchableItems.Add(
                new CatchableItem(
                    itemStack,
                    player.position,
                    new Sprite(itemStack.Item.Icon, GameContext.ScaleXY(1)),
                    GameContext
                )
            );
        }

        player.HealthChanged -= camera.damageShake;
        player.Inventory.UpdateInventoryUI -= HUD.inventoryUI.UpdateHandItem;

        HUD = null;
        player = null;
        deathPanel.enabled = true;
    }

    private void processPlayerSpawn(bool isNewGame)
    {
        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, GameContext.ScaleXY(1)), GameContext); 
        if (!isNewGame)
        {
            player.Restore(saveData.Player);
        }

        HUD = new HUD(player, GameContext, graphicsDevice, input, DIURNAL_PRESET_TIME);

        // update stats from beggining
        HUD.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);
        player.ForceUpdateStats();

        player.HealthChanged += camera.damageShake;
        player.Inventory.UpdateInventoryUI += HUD.inventoryUI.UpdateHandItem;

        if(isNewGame) player.position = world.FindSpawnPosition();
    }

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

}
