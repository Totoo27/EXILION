using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using EXILION.Items;

namespace EXILION.World;

public class ObjectDefinition
{
    public ObjectType Type { get; init; }
    public bool BlocksMovement { get; init; }   // Indica si el jugador puede atravesarlo.
    public float CollisionRadius { get; init; } // Qué tan grande es su zona de choque, medida en tiles.
    public int? MaxHealth { get; init; }
    public bool CanBeCollected { get; init; }
    public bool HandCanBreak { get; init; }
    public IReadOnlyList<Item> BreakableByItems { get; init; } = Array.Empty<Item>();
    public IReadOnlyList<ObjectDrop> Drops { get; init; } = new List<ObjectDrop>();
    public Rectangle Source { get; init; }      // Qué parte de la imagen Objects.png usamos.
    public float DrawWidthTiles { get; init; }  // Qué ancho tendrá al aparecer en el mundo.

    public bool CanBeBrokenBy(Item item)
    {
        if (item == null) return false;

        foreach (var allowedItem in BreakableByItems)
        {
            if (allowedItem.Id == item.Id)
                return true;
        }

        return false;
    }
}

public sealed record ObjectDrop(Item Item, int Quantity);
