
using EXILION.Entities.LivingThings;
using EXILION.Logic;
using EXILION.Network;
using EXILION.Saving;
using EXILION.World;
using Microsoft.Xna.Framework;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace EXILION.Server;

internal sealed class GameSession
{
    private readonly object sync = new();

    private readonly World.World world;
    private readonly GameClock clock = new();

    private readonly Dictionary<Guid, ServerPlayer> players = new();
    private readonly List<Creature> creatures = new();

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

        spawnGenerator = new CreatureSpawnGenerator(world);
    }

    public WorldSnapshot CreateSnapshot()
    {

        return new WorldSnapshot(
            tick,
            players.Values.Select(player =>
                new PlayerSnapshot(
                    player.Id,
                    player.Player.position.X,
                    player.Player.position.Y,
                    player.Player.VisualAngle,
                    player.Player.health,
                    player.Player.leftHand.Position.X,
                    player.Player.leftHand.Position.Y,
                    player.Player.leftHand.Angle,
                    player.Player.rightHand.Position.X,
                    player.Player.rightHand.Position.Y,
                    player.Player.rightHand.Angle
                )
            ).ToArray()
        );

    }

    public void AddPlayer(Guid playerId, string playerName)
    {
        lock (sync)
        {
            if (players.ContainsKey(playerId))
                return;

            Player player = new(
                Vector2.Zero,
                new Microsoft.Xna.Framework.Point(EntitySize, EntitySize)
            );

            player.position = world.FindSpawnPosition();

            players.Add(
                playerId,
                new ServerPlayer(playerId, playerName, player)
            );

            UpdateLoadedChunks();
        }
    }

    public void RemovePlayer(Guid playerId)
    {
        lock (sync)
        {
            players.Remove(playerId);
            UpdateLoadedChunks();
        }
    }

    public void SetPlayerInput(Guid playerId, PlayerInput input, Vector2 mousePosition)
    {
        lock (sync)
        {
            if (players.TryGetValue(playerId, out ServerPlayer? player))
            {
                player.Input = input;
                player.MousePosition = mousePosition;
            }
        }
    }

    public void Update(float deltaTime)
    {
        lock (sync)
        {
            if (players.Count == 0)
                return;
            tick++;
            clock.Update(deltaTime);

            world.UpdateObjects(deltaTime);

            foreach (ServerPlayer serverPlayer in players.Values)
            {
                serverPlayer.Player.Update(
                    serverPlayer.MousePosition,
                    serverPlayer.Input,
                    deltaTime,
                    world
                );
            }

            foreach (Creature creature in creatures)
            {
                ServerPlayer? target = players.Values
                    .OrderBy(p => Vector2.DistanceSquared(
                        p.Player.position,
                        creature.position
                    ))
                    .FirstOrDefault();

                if (target != null)
                {
                    creature.Update(
                        deltaTime,
                        world,
                        target.Player.position,
                        clock.IsNight
                    );
                }
            }

            creatures.RemoveAll(
                creature => creature.isDead || creature.ShouldDespawn
            );

            UpdateLoadedChunks();
        }
    }

    private void UpdateLoadedChunks()
    {
        world.UpdateAroundPositions(
            players.Values.Select(p => p.Player.position)
        );
    }

    private sealed class ServerPlayer(
        Guid id,
        string name,
        Player player)
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
        public Player Player { get; } = player;

        public PlayerInput Input { get; set; }
        public Vector2 MousePosition { get; set; } = player.position;
    }
}
