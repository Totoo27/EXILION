using System.Collections.Generic;

namespace EXILION.Saving;

public class WorldSaveData
{
    public int Version { get; set; } = 1;

    public string WorldId { get; set; } = string.Empty;
    public string WorldName { get; set; } = string.Empty;
    public int WorldSeed { get; set; }

    public double WorldTime { get; set; }

    public PlayerSaveData Player { get; set; } = new();

    public List<CatchableItemSaveData> CatchableItems { get; set; } = new();
}