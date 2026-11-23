using System;

namespace EXILION.Items;

public class Armor : Item
{
    // ArmorPoints is expressed as a percentage from 0 to 100.
    public float ArmorPoints { get; }
    public float SpeedMultiplier { get; }

    public Armor(int id, string name, float armorPoints, float speedMultiplier)
        : base(id, name, ItemType.ARMOR)
    {
        ArmorPoints = Math.Clamp(armorPoints, 0f, 100f);
        SpeedMultiplier = Math.Max(0f, speedMultiplier);
    }
}
