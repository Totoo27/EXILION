using System;
using System.Collections.Generic;
using System.Linq;
using EXILION.Items;

namespace EXILION.World;

public static class ObjectProperties
{
    private static readonly Dictionary<ObjectType, ObjectDefinition> Definitions = new()
    {
        { ObjectType.Tree, new ObjectDefinition {
            Type = ObjectType.Tree, BlocksMovement = true, CollisionRadius = 0.50f, MaxHealth = 20,
            HandCanBreak = true, BreakableByItems = new[] { ItemRegistry.HachaDeMadera, ItemRegistry.HachaDePiedra, ItemRegistry.HachaDeHierro, ItemRegistry.HachaDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Madera, 2) },
            } },

        { ObjectType.Rock, new ObjectDefinition {
            Type = ObjectType.Rock, BlocksMovement = true, CollisionRadius = 0.40f, MaxHealth = 20,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeMadera, ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Piedra, 2) },
            } },

        { ObjectType.Bush, new ObjectDefinition {
            Type = ObjectType.Bush, BlocksMovement = false, CollisionRadius = 0.35f, MaxHealth = 4,
            HandCanBreak = true,
            } },

        { ObjectType.FruitBush, new ObjectDefinition {
            Type = ObjectType.FruitBush, BlocksMovement = false, CollisionRadius = 0.35f, MaxHealth = 4,
            HandCanBreak = true,
            Drops = new[] { new ObjectDrop(ItemRegistry.Baya, 2) },
            } },

        { ObjectType.Iron, new ObjectDefinition {
            Type = ObjectType.Iron, BlocksMovement = true, CollisionRadius = 0.50f, MaxHealth = 30,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Hierro, 2) },
            } },

        { ObjectType.Titanium, new ObjectDefinition {
            Type = ObjectType.Titanium, BlocksMovement = true, CollisionRadius = 0.55f, MaxHealth = 45,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Titanio, 2) },
            } },

        { ObjectType.Thorium, new ObjectDefinition {
            Type = ObjectType.Thorium, BlocksMovement = true, CollisionRadius = 0.6f, MaxHealth = 60,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Torio, 2) },
            } },

        { ObjectType.Sandstone, new ObjectDefinition {
            Type = ObjectType.Sandstone, BlocksMovement = true, CollisionRadius = 0.35f, MaxHealth = 15,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDeMadera, ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Arena, 2) },
            } },

        { ObjectType.OxygenGenerator, new ObjectDefinition {
            Type = ObjectType.OxygenGenerator, BlocksMovement = true, CollisionRadius = 0.30f, MaxHealth = 30,
            HandCanBreak = false,
            OxygenRadiusTiles = 8f,
            BreakableByItems = new[] { ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.GeneradorO2, 1) } }},

        { ObjectType.CraftingTable, new ObjectDefinition {
            Type = ObjectType.CraftingTable, BlocksMovement = true, CollisionRadius = 0.45f, MaxHealth = 20,
            HandCanBreak = false,
            BreakableByItems = new[] { ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.CraftingTable, 1) },
            IsCraftingTable = true, InteractionRadiusTiles = 1.5f}
        },

        { ObjectType.SpaceShip, new ObjectDefinition {
            Type = ObjectType.SpaceShip, BlocksMovement = true, CollisionRadius = 1.2f, MaxHealth = 999,
            HandCanBreak = false,
            IsExit = true, InteractionRadiusTiles = 2f }},

        { ObjectType.WoodenFloor, new ObjectDefinition {
            Type = ObjectType.WoodenFloor, BlocksMovement = false, CollisionRadius = 0.35f, MaxHealth = 50,
            HandCanBreak = true, BreakableByItems = new[] { ItemRegistry.HachaDeMadera, ItemRegistry.HachaDePiedra, ItemRegistry.HachaDeHierro, ItemRegistry.HachaDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Madera, 1) },
            }},

        { ObjectType.WoodenWall, new ObjectDefinition {
            Type = ObjectType.WoodenWall, BlocksMovement = true, CollisionRadius = 0.5f, MaxHealth = 50,
            HandCanBreak = true, BreakableByItems = new[] { ItemRegistry.HachaDeMadera, ItemRegistry.HachaDePiedra, ItemRegistry.HachaDeHierro, ItemRegistry.HachaDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Madera, 1) },
            }},

        { ObjectType.StoneWall, new ObjectDefinition {
            Type = ObjectType.StoneWall, BlocksMovement = true, CollisionRadius = 0.5f, MaxHealth = 80,
            HandCanBreak = true, BreakableByItems = new[] { ItemRegistry.PicoDeMadera, ItemRegistry.PicoDePiedra, ItemRegistry.PicoDeHierro, ItemRegistry.PicoDeTitanio },
            Drops = new[] { new ObjectDrop(ItemRegistry.Piedra, 1) },
            }},
            
    };

    public static readonly float MaxCollisionRadius = Definitions.Values.Max(d => d.CollisionRadius);

    public static ObjectDefinition Get(ObjectType type) => type switch
    {
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
