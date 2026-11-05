using System.Collections.Generic;
using EXILION.Entities.LivingThings;

namespace EXILION.Saving;

public class PlayerSaveData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public int Health { get; set; }
    public PlayerStat Hunger { get; set; }
    public PlayerStat Thirst { get; set; }
    public PlayerStat Oxygen { get; set; }

    public List<InventorySlotSaveData> Inventory { get; set; } = new();

}