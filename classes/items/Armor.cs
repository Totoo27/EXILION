using System;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Items;

public class Armor : Item
{
    // ArmorPoints is expressed as a percentage from 0 to 100.
    public float ArmorPoints { get; }
    public float SpeedMultiplier { get; }

    public Armor(int id, string name, float armorPoints, float speedMultiplier, Texture2D icon = null)
        : base(id, name, ItemType.ARMOR, icon)
    {
        ArmorPoints = Math.Clamp(armorPoints, 0f, 100f);
        SpeedMultiplier = Math.Max(0f, speedMultiplier);
    }
}
