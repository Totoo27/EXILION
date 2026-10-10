using System.Collections.Generic;

namespace EXILION.Saving;

public class PlayerSaveData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public float Health { get; set; }
    public float Hunger { get; set; }
    public float Thirst { get; set; }
    public float Oxygen { get; set; }

    public List<InventorySlotSaveData> Inventory { get; set; } = new();
}