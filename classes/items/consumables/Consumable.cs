using EXILION.Entities.LivingThings;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Items;

public class Consumable : Item
{
    public int statRestore { get; }
    public StatType statType { get; private set; }

    public Consumable(int id, string name, int statRestore, StatType statType, Texture2D icon = null)
        : base(id, name, ItemType.CONSUMABLE, icon, true)
    {
        this.statRestore = statRestore;
        this.statType = statType;
    }
}