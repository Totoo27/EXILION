using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace EXILION.World;

public static class ObjectProperties
{
    private static readonly Dictionary<ObjectType, ObjectDefinition> Definitions = new()
    {
        { ObjectType.Tree, new ObjectDefinition {
            Type = ObjectType.Tree, BlocksMovement = true, CollisionRadius = 0.20f,
            Source = new Rectangle(0, 0, 32, 48), DrawWidthTiles = 1.6f } },

        { ObjectType.Rock, new ObjectDefinition {
            Type = ObjectType.Rock, BlocksMovement = true, CollisionRadius = 0.35f,
            Source = new Rectangle(32, 0, 32, 48), DrawWidthTiles = 1.0f } },

        { ObjectType.Bush, new ObjectDefinition {
            Type = ObjectType.Bush, BlocksMovement = false, CollisionRadius = 0.30f,
            Source = new Rectangle(64, 0, 32, 48), DrawWidthTiles = 1.0f } },
    };

    public static readonly float MaxCollisionRadius = Definitions.Values.Max(d => d.CollisionRadius);

    public static ObjectDefinition Get(ObjectType type) => Definitions[type];

    // Probabilidad de que aparezca cada objeto en un tile de cada bioma
    private static readonly (ObjectType Type, float Chance)[] ForestRules = { (ObjectType.Tree, 0.22f), (ObjectType.Bush, 0.06f) };
    private static readonly (ObjectType Type, float Chance)[] GrassRules  = { (ObjectType.Tree, 0.02f), (ObjectType.Bush, 0.04f), (ObjectType.Rock, 0.01f) };
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
        _ => NoRules, // agua: nunca hay objetos
    };
}