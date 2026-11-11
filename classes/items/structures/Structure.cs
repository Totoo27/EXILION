using System;
using Microsoft.Xna.Framework.Graphics;
using EXILION.World;

namespace EXILION.Items;

public class Structure : Item
{
    public ObjectType objectType;
    public Structure(int id, string name,ObjectType type, Texture2D icon = null )
        : base(id, name, ItemType.STRUCTURES, icon)
    {
        this.objectType = type;
    }
}