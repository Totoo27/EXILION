namespace EXILION.Items;

public class Item
{
    public const int MaxStackSize = 64;

    public int Id { get; }
    public string Name { get; }
    public ItemType Type { get; }
    public bool TwoHanded { get; set; }

    public Item(int id, string name, ItemType type, bool twoHanded = false)
    {
        Id = id;
        Name = name;
        Type = type;
        TwoHanded = twoHanded;
    }

     public int GetMaxStackSize() => MaxStackSize;

}
