using System.Collections.Generic;

namespace EXILION.Saving;

public class WorldSaveData
{
    public int Version { get; set; } = 1;

    public string WorldId { get; set; } = string.Empty;
    public string WorldName { get; set; } = string.Empty;
    public bool IsNewWorld { get; set; } = true;
    public int WorldSeed { get; set; }
    public int WorldTime { get; set; }
    public bool IsNight { get; set; }

    public PlayerSaveData Player { get; set; } = new();

    public List<CatchableItemSaveData> CatchableItems { get; set; } = new();
    public List<WorldObjectStateSaveData> WorldObjectStates { get; set; } = new();
}
