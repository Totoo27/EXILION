using System;
using EXILION.World;

namespace EXILION.Items;

public class Structure : Item
{
    public ObjectType objectType;
    public Structure(int id, string name,ObjectType type)
        : base(id, name, ItemType.STRUCTURES, true)
    {
        this.objectType = type;
    }
}
