using System.Collections.Generic;
using EXILION.Entities.LivingThings;

namespace EXILION.Saving;

public class CreatureSaveData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; } = 40;
    public CreatureType Type { get; set; }

}
