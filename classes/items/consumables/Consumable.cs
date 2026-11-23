using EXILION.Entities.LivingThings;

namespace EXILION.Items;

public class Consumable : Item
{
    public int statRestore { get; }
    public StatType statType { get; private set; }

    public Consumable(int id, string name, int statRestore, StatType statType)
        : base(id, name, ItemType.CONSUMABLE, true)
    {
        this.statRestore = statRestore;
        this.statType = statType;
    }
}
