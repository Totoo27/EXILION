using EXILION.World;

namespace EXILION.Saving;

public sealed class WorldObjectStateSaveData
{
    public int Seed { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public int SpawnIndex { get; set; }
    public int? CurrentHealth { get; set; }
    public bool IsDestroyed { get; set; }
    public bool IsCollected { get; set; }

    public static WorldObjectStateSaveData FromState(WorldObjectState state) => new()
    {
        Seed = state.Id.Seed,
        TileX = state.Id.TileX,
        TileY = state.Id.TileY,
        SpawnIndex = state.Id.SpawnIndex,
        CurrentHealth = state.CurrentHealth,
        IsDestroyed = state.IsDestroyed,
        IsCollected = state.IsCollected
    };

    public WorldObjectState ToState() => new(
        new WorldObjectId(Seed, TileX, TileY, SpawnIndex),
        CurrentHealth,
        IsDestroyed,
        IsCollected);
}
