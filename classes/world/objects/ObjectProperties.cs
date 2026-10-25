using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using EXILION.Items;

namespace EXILION.World;

public static class ObjectProperties
{
    private static readonly Dictionary<ObjectType, ObjectDefinition> Definitions = new()
    {
        { ObjectType.Tree, new ObjectDefinition {
            Type = ObjectType.Tree, BlocksMovement = true, CollisionRadius = 0.55f, MaxHealth = 15,
            HandCanBreak = true, AbovePlayer = true,
            BreakableByItems = new[] { ItemRegistry.HachaTest },
            Drops = new[] { new ObjectDrop(ItemRegistry.Madera, 2) },
            Source = new Rectangle(64, 0, 64, 64), DrawWidthTiles = 2.2f } },

        { ObjectType.Rock, new ObjectDefinition {
            Type = ObjectType.Rock, BlocksMovement = true, CollisionRadius = 0.45f, MaxHealth = 10,
            HandCanBreak = false, AbovePlayer = false,
            Drops = new[] { new ObjectDrop(ItemRegistry.Piedra, 2) },
            Source = new Rectangle(128, 0, 64, 64), DrawWidthTiles = 1.6f } },

        { ObjectType.Bush, new ObjectDefinition {
            Type = ObjectType.Bush, BlocksMovement = false, CollisionRadius = 0.30f, MaxHealth = 1,
            HandCanBreak = true, AbovePlayer = false,
            Source = new Rectangle(0, 0, 64, 64), DrawWidthTiles = 1.5f } },
    };

    public static readonly float MaxCollisionRadius = Definitions.Values.Max(d => d.CollisionRadius);

    public static ObjectDefinition Get(ObjectType type) => Definitions[type];

    // Estas chances indican qué tan seguido aparece cada objeto en cada tipo de terreno.
    private static readonly (ObjectType Type, float Chance)[] ForestRules = { (ObjectType.Tree, 0.18f), (ObjectType.Bush, 0.005f) };
    private static readonly (ObjectType Type, float Chance)[] GrassRules  = { (ObjectType.Tree, 0.02f), (ObjectType.Bush, 0.02f), (ObjectType.Rock, 0.01f) };
    private static readonly (ObjectType Type, float Chance)[] SandRules   = { (ObjectType.Rock, 0.015f) };
    private static readonly (ObjectType Type, float Chance)[] RockRules   = { (ObjectType.Rock, 0.10f) };
    private static readonly (ObjectType Type, float Chance)[] SnowRules   = { (ObjectType.Rock, 0.03f) };
    private static readonly (ObjectType Type, float Chance)[] NoRules     = Array.Empty<(ObjectType, float)>();

    public static (ObjectType Type, float Chance)[] GetSpawnRules(TileType biome) => biome switch
    {
        TileType.Forest => ForestRules,
        TileType.Grass  => GrassRules,
        TileType.Sand   => SandRules,
        TileType.Rock   => RockRules,
        TileType.Snow   => SnowRules,
        _ => NoRules, // En el agua no aparecen objetos.
    };
}
