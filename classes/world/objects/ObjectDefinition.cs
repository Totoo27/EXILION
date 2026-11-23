using System;
using System.Collections.Generic;
using EXILION.Items;

namespace EXILION.World;

public class ObjectDefinition
{
    public ObjectType Type { get; init; }
    public bool BlocksMovement { get; init; }
    public float CollisionRadius { get; init; }
    public int? MaxHealth { get; init; }
    public bool HandCanBreak { get; init; }
    public IReadOnlyList<Item> BreakableByItems { get; init; } = Array.Empty<Item>();
    public IReadOnlyList<ObjectDrop> Drops { get; init; } = new List<ObjectDrop>();
    public float OxygenRadiusTiles { get; init; }

    public bool IsCraftingTable { get; init; }
    public bool IsExit { get; init; }
    public float InteractionRadiusTiles { get; init; }

    public bool CanBeBrokenBy(Item item)
    {
        foreach (var allowedItem in BreakableByItems)
        {
            if (allowedItem.Id == item.Id)
                return true;
        }

        return false;
    }
}

public sealed record ObjectDrop(Item Item, int Quantity);
