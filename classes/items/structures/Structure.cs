using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Items;

public class Structure : Item
{
    public Structure(int id, string name, Texture2D icon = null)
        : base(id, name, ItemType.STRUCTURES, icon)
    {
    }
}