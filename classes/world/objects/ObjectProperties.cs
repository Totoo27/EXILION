using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using EXILION.Items;
using EXILION;

namespace EXILION.World;

public static class ObjectProperties
{
    private static readonly Dictionary<ObjectType, ObjectDefinition> Definitions = new()
    {
        { ObjectType.Tree, new ObjectDefinition {
            Type = ObjectType.Tree, BlocksMovement = true, CollisionRadius = 0.50f, MaxHealth = 20,
            HitSound = Assets.SoundEffects?.chop,
            AbovePlayer = true,
            HandCanBreak = true, BreakableByItems = new[] { ItemRegistry.HachaDeMadera, ItemRegistry.HachaDePiedra },
            Drops = new[] { new ObjectDrop(ItemRegistry.Madera, 2) },
            Source = new Rectangle(128, 0, 64, 64), DrawWidthTiles = 2.2f } },

        { ObjectType.Rock, new ObjectDefinition {
            Type = ObjectType.Rock, BlocksMovement = true, CollisionRadius = 0.40f, MaxHealth = 20,
            HitSound = Assets.SoundEffects?.mine,
            AbovePlayer = false,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeMadera, ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Piedra, 2) },
            Source = new Rectangle(192, 0, 64, 64), DrawWidthTiles = 1.6f } },

        { ObjectType.Bush, new ObjectDefinition {
            Type = ObjectType.Bush, BlocksMovement = false, CollisionRadius = 0.35f, MaxHealth = 4,
            HitSound = Assets.SoundEffects?.brokenBush,
            AbovePlayer = false,
            HandCanBreak = true,
            Source = new Rectangle(64, 0, 64, 64), DrawWidthTiles = 1.4f } },

        { ObjectType.FruitBush, new ObjectDefinition {
            Type = ObjectType.FruitBush, BlocksMovement = false, CollisionRadius = 0.35f, MaxHealth = 4,
            HitSound = Assets.SoundEffects?.brokenBush,
            AbovePlayer = false,
            HandCanBreak = true,
            Drops = new[] { new ObjectDrop(ItemRegistry.Baya, 2) },
            Source = new Rectangle(0, 0, 64, 64), DrawWidthTiles = 1.4f } },

        { ObjectType.Iron, new ObjectDefinition {
            Type = ObjectType.Iron, BlocksMovement = true, CollisionRadius = 0.50f, MaxHealth = 30,
            HitSound = Assets.SoundEffects?.mine,
            AbovePlayer = false,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Hierro, 2) },
            Source = new Rectangle(256, 0, 64, 64), DrawWidthTiles = 1.7f } },

        { ObjectType.Titanium, new ObjectDefinition {
            Type = ObjectType.Titanium, BlocksMovement = true, CollisionRadius = 0.55f, MaxHealth = 45,
            HitSound = Assets.SoundEffects?.mine,
            AbovePlayer = false,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Titanio, 2) },
            Source = new Rectangle(256, 64, 64, 64), DrawWidthTiles = 1.9f } },

        { ObjectType.Thorium, new ObjectDefinition {
            Type = ObjectType.Thorium, BlocksMovement = true, CollisionRadius = 0.6f, MaxHealth = 60,
            HitSound = Assets.SoundEffects?.mine,
            AbovePlayer = false,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Torio, 2) },
            Source = new Rectangle(192, 64, 64, 64), DrawWidthTiles = 2.2f } },

        { ObjectType.Sandstone, new ObjectDefinition {
            Type = ObjectType.Sandstone, BlocksMovement = true, CollisionRadius = 0.35f, MaxHealth = 15,
            HitSound = Assets.SoundEffects?.mine,
            AbovePlayer = false,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeMadera, ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Arena, 2) },
            Source = new Rectangle(0, 64, 64, 64), DrawWidthTiles = 1.4f } },
    };

    private static readonly ObjectDefinition BlockDefinition = new ObjectDefinition {
        Type = ObjectType.Block, BlocksMovement = true, CollisionRadius = 0.42f, MaxHealth = 20,
        HitSound = Assets.SoundEffects?.mine, AbovePlayer = false, HandCanBreak = true,
        Source = new Rectangle(0, 0, 32, 32), DrawWidthTiles = 1f };

    private static readonly ObjectDefinition OxygenGeneratorDefinition = new ObjectDefinition {
        Type = ObjectType.OxygenGenerator, BlocksMovement = true, CollisionRadius = 0.45f, MaxHealth = 30,
        HitSound = Assets.SoundEffects?.mine, AbovePlayer = false, HandCanBreak = true,
        UsesGeneratorSprite = true, OxygenRadiusTiles = 8f, OxygenCircleColor = Color.LightBlue,
        OxygenCircleAlpha = 80, Source = new Rectangle(0, 0, 32, 32), DrawWidthTiles = 1f };

    public static readonly float MaxCollisionRadius = Definitions.Values.Max(d => d.CollisionRadius);

    public static ObjectDefinition Get(ObjectType type) => type switch
    {
        ObjectType.Block => BlockDefinition,
        ObjectType.OxygenGenerator => OxygenGeneratorDefinition,
        _ => Definitions[type]
    };

    // Estas chances indican qué tan seguido aparece cada objeto en cada tipo de terreno.
    private static readonly (ObjectType Type, float Chance)[] ForestRules = { (ObjectType.Tree, 0.22f), (ObjectType.Bush, 0.04f), (ObjectType.FruitBush, 0.008f)  };
    private static readonly (ObjectType Type, float Chance)[] GrassRules  = { (ObjectType.Tree, 0.02f), (ObjectType.Bush, 0.01f), (ObjectType.FruitBush, 0.01f), (ObjectType.Rock, 0.01f) };
    private static readonly (ObjectType Type, float Chance)[] SandRules   = { (ObjectType.Rock, 0.015f), (ObjectType.Sandstone, 0.025f) };
    private static readonly (ObjectType Type, float Chance)[] RockRules   = { (ObjectType.Rock, 0.06f), (ObjectType.Iron, 0.03f), (ObjectType.Titanium, 0.015f), (ObjectType.Thorium, 0.005f) };
    private static readonly (ObjectType Type, float Chance)[] SnowRules   = { (ObjectType.Rock, 0.03f), (ObjectType.Titanium, 0.02f), (ObjectType.Thorium, 0.009f) };
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
