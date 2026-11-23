
using EXILION.Entities.LivingThings;
using EXILION.Logic;
using EXILION.Network;
using EXILION.Saving;
using EXILION.World;
using EXILION.Items;
using EXILION.Entities.CatchableItems;
using Microsoft.Xna.Framework;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace EXILION.Server;

internal sealed class GameSession
{
    private readonly object sync = new();
    private readonly WorldSaveData saveData;

    private readonly World.World world;
    private readonly GameClock clock = new();

    private readonly Dictionary<Guid, ServerPlayer> players = new();
    private readonly List<Creature> creatures = new();
    private readonly List<CatchableItem> catchableItems = new();
    private readonly GameWorldLogic gameLogic = new();
    private readonly List<PendingWorldHit> worldHitEvents = new();
    private int nextWorldHitEventId;

    private readonly CreatureSpawnGenerator spawnGenerator;

    private const int TileSize = 64;
    private const int EntitySize = 40;

    public string WorldId { get; }
    public string WorldName { get; }
    public int WorldSeed { get; }
    public int Tick => tick;

    private int tick = 0;

    public GameSession(WorldSaveData saveData)
    {
        this.saveData = saveData;
        this.saveData.PlayerProfiles ??= new Dictionary<Guid, PlayerSaveData>();
        WorldId = saveData.WorldId;
        WorldName = saveData.WorldName;
        WorldSeed = saveData.WorldSeed;

        world = new World.World(
            seed: WorldSeed,
            tileSize: TileSize
        )
        {
            RenderDistanceChunks = 2
        };

        world.RestoreObjectStates(
            saveData.WorldObjectStates.Select(x => x.ToState())
        );
        world.ObjectDestroyed += DropWorldObjectItems;
        world.ObjectDamaged += RecordWorldObjectHit;

        if (saveData.IsNewWorld && saveData.WorldTime <= 0)
        {
            saveData.WorldTime = GameClock.DayLengthSeconds;
            saveData.IsNight = false;
        }

        clock.Restore(
            saveData.WorldTime,
            saveData.DayCount,
            saveData.IsNight
        );

        foreach (CreatureSaveData data in saveData.Creatures)
        {
            creatures.Add(
                CreatureFactory.CreateRestoredCreature(
                    data,
                    new Microsoft.Xna.Framework.Point(EntitySize, EntitySize)
                )
            );
        }

        foreach (CatchableItemSaveData data in saveData.CatchableItems)
        {
            Item? item = ItemRegistry.GetById(data.ItemId);
            if (item != null && data.Quantity > 0)
                catchableItems.Add(new CatchableItem(new ItemStack(item, data.Quantity),
                    new Vector2(data.PositionX, data.PositionY), new Microsoft.Xna.Framework.Point(28, 28)));
        }

        if (saveData.IsNewWorld && catchableItems.Count == 0)
        {
            catchableItems.Add(new CatchableItem(new ItemStack(ItemRegistry.GeneradorO2, 1),
                Vector2.Zero, new Microsoft.Xna.Framework.Point(28, 28)));
        }

        spawnGenerator = new CreatureSpawnGenerator(world);
    }

    public WorldSnapshot CreateSnapshot()
    {
        lock (sync)
        {
            worldHitEvents.RemoveAll(hit => tick - hit.Tick > 60);
            return new WorldSnapshot(
            tick,
            players.Values.Select(player =>
                new PlayerSnapshot(
                    player.Id,
                    player.Player.position.X,
                    player.Player.position.Y,
                    player.Player.VisualAngle,
                    player.Player.health,
                    player.Player.hunger,
                    player.Player.thirst,
                    player.Player.oxygen,
                    player.Player.leftHand.Position.X,
                    player.Player.leftHand.Position.Y,
                    player.Player.leftHand.Angle,
                    player.Player.rightHand.Position.X,
                    player.Player.rightHand.Position.Y,
                    player.Player.rightHand.Angle,
                    player.Player.leftHand.HeldItem?.Id ?? -1,
                    player.Player.leftHand.ItemPosition.X,
                    player.Player.leftHand.ItemPosition.Y,
                    player.Player.leftHand.ItemAngle,
                    player.Player.rightHand.HeldItem?.Id ?? -1,
                    player.Player.rightHand.ItemPosition.X,
                    player.Player.rightHand.ItemPosition.Y,
                    player.Player.rightHand.ItemAngle,
                    player.IsDead
                )
            ).ToArray(),
            world.CaptureObjectStates().ToArray(),
            creatures.Where(creature => !creature.isDead && !creature.ShouldDespawn)
                .Select(creature => new CreatureSnapshot(creature.NetworkId, creature.position.X, creature.position.Y,
                    creature.VisualAngle, creature.health, creature.Type)).ToArray(),
            catchableItems.Where(item => !item.Picked)
                .Select(item => new CatchableItemSnapshot(item.NetworkId, item.Stack.Item.Id, item.Stack.Quantity,
                    item.position.X, item.position.Y)).ToArray(),
            worldHitEvents.Select(hit => new WorldObjectHitEvent(hit.EventId, hit.ObjectId, hit.Type)).ToArray(),
            clock.RemainingSeconds,
            clock.DayCount,
            clock.IsNight
            );
        }
    }

    public void AddPlayer(Guid playerId, Guid persistentPlayerId, string playerName)
    {
        lock (sync)
        {
            if (players.ContainsKey(playerId))
                return;

            Player player = new(
                Vector2.Zero,
                new Microsoft.Xna.Framework.Point(EntitySize, EntitySize)
            );
            if (saveData.PlayerProfiles.TryGetValue(persistentPlayerId, out PlayerSaveData? savedPlayer))
                player.Restore(savedPlayer);
            else
                player.position = world.FindSpawnPosition();

            players.Add(
                playerId,
                new ServerPlayer(playerId, persistentPlayerId, playerName, player)
            );

            UpdateLoadedChunks();
        }
    }

    public void RemovePlayer(Guid playerId)
    {
        lock (sync)
        {
            if (players.Remove(playerId, out ServerPlayer? player))
                saveData.PlayerProfiles[player.PersistentId] = player.Player.GetSaveData();
            UpdateLoadedChunks();
        }
    }

    public PlayerSaveData GetPlayerSaveData(Guid playerId)
    {
        lock (sync)
        {
            if (!players.TryGetValue(playerId, out ServerPlayer? player))
                throw new InvalidOperationException("La sesión del jugador no existe.");
            return player.Player.GetSaveData();
        }
    }

    public WorldSaveData CaptureSaveData()
    {
        lock (sync)
        {
            foreach (ServerPlayer player in players.Values)
                saveData.PlayerProfiles[player.PersistentId] = player.Player.GetSaveData();
            return new WorldSaveData
            {
                Version = saveData.Version,
                WorldId = WorldId,
                WorldName = WorldName,
                IsNewWorld = false,
                WorldSeed = WorldSeed,
                WorldTime = clock.RemainingSeconds,
                DayCount = clock.DayCount,
                IsNight = clock.IsNight,
                Player = saveData.Player,
                PlayerProfiles = new Dictionary<Guid, PlayerSaveData>(saveData.PlayerProfiles),
                WorldObjectStates = world.CaptureObjectStates()
                    .Select(WorldObjectStateSaveData.FromState).ToList(),
                Creatures = creatures.Where(creature => !creature.isDead && !creature.ShouldDespawn)
                    .Select(creature => creature.GetSaveData()).ToList(),
                CatchableItems = catchableItems.Where(item => !item.Picked)
                    .Select(item => item.GetSaveData()).ToList()
            };
        }
    }

    public void Respawn(Guid playerId)
    {
        lock (sync)
        {
            if (!players.TryGetValue(playerId, out ServerPlayer? serverPlayer) || !serverPlayer.IsDead)
                return;

            Player player = new(Vector2.Zero, new Microsoft.Xna.Framework.Point(EntitySize, EntitySize))
            {
                position = world.FindSpawnPosition()
            };
            serverPlayer.Player = player;
            serverPlayer.IsDead = false;
            serverPlayer.Input = default;
            serverPlayer.MousePosition = player.position;
            serverPlayer.HeldItemId = -1;
            serverPlayer.PlaceBlockRequested = false;
            UpdateLoadedChunks();
        }
    }

    public void SetPlayerInput(Guid playerId, PlayerInput input, Vector2 mousePosition, int heldItemId, bool placeBlockRequested)
    {
        lock (sync)
        {
            if (players.TryGetValue(playerId, out ServerPlayer? player))
            {
                player.Input = player.Input with
                {
                    MoveUp = input.MoveUp,
                    MoveDown = input.MoveDown,
                    MoveLeft = input.MoveLeft,
                    MoveRight = input.MoveRight,
                    Run = input.Run,
                    PrimaryActionCaptured = input.PrimaryActionCaptured,
                    PrimaryActionPressed = player.Input.PrimaryActionPressed || input.PrimaryActionPressed,
                    InteractPressed = player.Input.InteractPressed || input.InteractPressed
                };
                player.MousePosition = mousePosition;
                player.HeldItemId = heldItemId;
                player.PlaceBlockRequested |= placeBlockRequested;
            }
        }
    }

    public void DropItem(Guid playerId, int itemId, int quantity)
    {
        lock (sync)
        {
            if (!players.TryGetValue(playerId, out ServerPlayer? player)) return;
            Item? item = ItemRegistry.GetById(itemId);
            if (item == null || quantity is < 1 or > 9999) return;
            player.Player.Inventory.RemoveItem(item, quantity);
            Vector2 position = new(player.Player.position.X, player.Player.GetHitbox().Bottom + 16);
            catchableItems.Add(new CatchableItem(new ItemStack(item, quantity), position,
                new Microsoft.Xna.Framework.Point(28, 28)));
        }
    }

    public bool Update(float deltaTime)
    {
        lock (sync)
        {
            tick++;
            if (players.Count == 0)
                return false;
            bool wasNight = clock.IsNight;
            bool phaseChanged = clock.Update(deltaTime);

            ServerPlayer[] activePlayers = players.Values.Where(serverPlayer => !serverPlayer.IsDead).ToArray();
            Player[] activePlayerCharacters = activePlayers.Select(serverPlayer => serverPlayer.Player).ToArray();
            if (activePlayers.Length > 0)
            {
                Vector2 spawnAnchor = activePlayers[0].Player.position;
                world.UpdateRegrowth(deltaTime, spawnAnchor);
                world.UpdateObjects(deltaTime);

                foreach (ServerPlayer serverPlayer in activePlayers)
                {
                    PlayerInput frameInput = serverPlayer.Input;
                    bool placeBlock = serverPlayer.PlaceBlockRequested;
                    serverPlayer.Input = serverPlayer.Input with { PrimaryActionPressed = false, InteractPressed = false };
                    serverPlayer.PlaceBlockRequested = false;
                    Item? heldItem = ItemRegistry.GetById(serverPlayer.HeldItemId);
                    if (serverPlayer.Player.rightHand.HeldItem?.Id != heldItem?.Id)
                        serverPlayer.Player.SetHandItemImmediately(heldItem);
                    serverPlayer.Player.Update(serverPlayer.MousePosition, frameInput, deltaTime, world);
                    gameLogic.ResolvePlayerAttacks(serverPlayer.Player, world, creatures, activePlayerCharacters);
                    if (frameInput.InteractPressed)
                        gameLogic.TryPickupItems(serverPlayer.Player, catchableItems);
                    if (placeBlock && heldItem is Structure)
                        gameLogic.TryPlaceStructure(serverPlayer.Player, world, serverPlayer.MousePosition);
                    gameLogic.ProcessCreatureContactDamage(serverPlayer.Player, creatures, deltaTime);
                    if (serverPlayer.Player.isDead) serverPlayer.IsDead = true;
                }

                foreach (Creature creature in creatures)
                {
                    ServerPlayer? target = activePlayers
                        .Where(serverPlayer => !serverPlayer.Player.isDead)
                        .OrderBy(serverPlayer => Vector2.DistanceSquared(serverPlayer.Player.position, creature.position))
                        .FirstOrDefault();

                    if (target != null)
                    {
                        creature.Update(deltaTime, world, target.Player.position, clock.IsNight);
                    }
                }

                Creature spawned = spawnGenerator.TrySpawn(deltaTime, spawnAnchor, clock.IsNight, clock.DayCount,
                    creatures, EntitySize / 2,
                    (type, position) => CreatureFactory.CreateCreature(type, position,
                        new Microsoft.Xna.Framework.Point(EntitySize, EntitySize)));
                if (spawned != null) creatures.Add(spawned);

                UpdateLoadedChunks();
            }

            for (int index = creatures.Count - 1; index >= 0; index--)
            {
                if (!creatures[index].isDead) continue;
                DropCreatureItems(creatures[index]);
                creatures.RemoveAt(index);
            }
            foreach (CatchableItem item in catchableItems) item.Update(deltaTime);
            catchableItems.RemoveAll(item => item.Picked);
            creatures.RemoveAll(creature => creature.ShouldDespawn);
            return phaseChanged && wasNight && !clock.IsNight;
        }
    }

    private void RecordWorldObjectHit(WorldObjectId objectId, ObjectType type)
        => worldHitEvents.Add(new PendingWorldHit(++nextWorldHitEventId, tick, objectId, type));

    private void DropWorldObjectItems(WorldObject worldObject)
    {
        Vector2 position = new(worldObject.TileX * world.TileSize, worldObject.TileY * world.TileSize);
        foreach (ObjectDrop drop in worldObject.Definition.Drops)
            if (drop.Quantity > 0)
                catchableItems.Add(new CatchableItem(new ItemStack(drop.Item, drop.Quantity), position,
                    new Microsoft.Xna.Framework.Point(28, 28)));
    }

    private void DropCreatureItems(Creature creature)
    {
        IReadOnlyList<ItemStack>? drops = creature.GetDropItems();
        if (drops == null) return;
        foreach (ItemStack drop in drops)
            if (drop != null && drop.Quantity > 0)
                catchableItems.Add(new CatchableItem(drop, creature.position, new Microsoft.Xna.Framework.Point(28, 28)));
    }

    private void UpdateLoadedChunks()
    {
        world.UpdateAroundPositions(
            players.Values.Select(p => p.Player.position)
        );
    }

    private sealed class ServerPlayer(
        Guid id,
        Guid persistentId,
        string name,
        Player player)
    {
        public Guid Id { get; } = id;
        public Guid PersistentId { get; } = persistentId;
        public string Name { get; } = name;
        public Player Player { get; set; } = player;

        public PlayerInput Input { get; set; }
        public Vector2 MousePosition { get; set; } = player.position;
        public int HeldItemId { get; set; } = -1;
        public bool PlaceBlockRequested { get; set; }
        public bool IsDead { get; set; }
    }

    private sealed record PendingWorldHit(int EventId, int Tick, WorldObjectId ObjectId, ObjectType Type);
}
