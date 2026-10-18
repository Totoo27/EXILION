using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Items;

public class Item
{
    public const int MaxStackSize = 64;

    public int Id { get; }
    public string Name { get; }
    public ItemType Type { get; }
    public Texture2D Icon { get; }       
    public bool TwoHanded { get; set; }

    public Item(int id, string name, ItemType type, Texture2D icon = null, bool twoHanded = false)
    {
        Id = id;
        Name = name;
        Type = type;
        Icon = icon;
        TwoHanded = twoHanded;
    }

     public int GetMaxStackSize() => MaxStackSize;

}