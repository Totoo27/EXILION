using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Items;

public class Consumable : Item
{
    public int ThirstRestore { get; }

    public Consumable(int id, string name, int thirstRestore, Texture2D icon = null)
        : base(id, name, ItemType.CONSUMABLE, icon)
    {
        ThirstRestore = thirstRestore;
    }
}