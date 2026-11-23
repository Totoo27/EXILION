using System.Collections.Generic;
using Microsoft.Xna.Framework;
using EXILION.World;

namespace EXILION.Client;

public sealed record ObjectVisualDefinition(Rectangle Source, float DrawWidthTiles, bool AbovePlayer,
    bool UseItemSprite = false, Color? OxygenCircleColor = null, byte OxygenCircleAlpha = 120);

/// <summary>Client-only rendering metadata keyed by stable world object type.</summary>
public static class ObjectVisuals
{
    private static readonly Dictionary<ObjectType, ObjectVisualDefinition> Definitions = new()
    {
        [ObjectType.Tree] = new(new(128, 0, 64, 64), 2.2f, true),
        [ObjectType.Rock] = new(new(192, 0, 64, 64), 1.6f, false),
        [ObjectType.Bush] = new(new(64, 0, 64, 64), 1.4f, false),
        [ObjectType.FruitBush] = new(new(0, 0, 64, 64), 1.4f, false),
        [ObjectType.Iron] = new(new(256, 0, 64, 64), 1.7f, false),
        [ObjectType.Titanium] = new(new(256, 64, 64, 64), 1.9f, false),
        [ObjectType.Thorium] = new(new(192, 64, 64, 64), 2.2f, false),
        [ObjectType.Sandstone] = new(new(0, 64, 64, 64), 1.4f, false),
        [ObjectType.OxygenGenerator] = new(Rectangle.Empty, 1f, false, true),
        [ObjectType.CraftingTable] = new(Rectangle.Empty, 1f, false, true),
        [ObjectType.SpaceShip] = new(Rectangle.Empty, 5.5f, true, true),
        [ObjectType.WoodenFloor] = new(Rectangle.Empty, 1f, false, true),
        [ObjectType.WoodenWall] = new(Rectangle.Empty, 1f, true, true),
        [ObjectType.StoneWall] = new(Rectangle.Empty, 1f, true, true)
    };

    public static ObjectVisualDefinition Get(ObjectType type) => Definitions[type];
}
