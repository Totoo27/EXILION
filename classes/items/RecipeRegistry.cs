using System.Collections.Generic;

namespace EXILION.Items;

public static class RecipeRegistry
{
    #nullable enable

    public static readonly Recipe WoodenAxeRecipe = new Recipe(
        id: 1,
        result: ItemRegistry.HachaDeMadera,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 4)
        },
        amount: 1,
        requiresCraftingTable: false
    );

    public static readonly Recipe WoodenPickaxeRecipe = new Recipe(
        id: 2,
        result: ItemRegistry.PicoDeMadera,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 8)
        },
        amount: 1,
        requiresCraftingTable: false
    );

    public static readonly Recipe StoneAxeRecipe = new Recipe(
        id: 3,
        result: ItemRegistry.HachaDePiedra,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.HachaDeMadera, 1),
            new Ingredient(ItemRegistry.Madera, 6),
            new Ingredient(ItemRegistry.Piedra, 6)
        },
        amount: 1,
        requiresCraftingTable: false
    );

    public static readonly Recipe StonePickaxeRecipe = new Recipe(
        id: 4,
        result: ItemRegistry.PicoDePiedra,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.PicoDeMadera, 1),
            new Ingredient(ItemRegistry.Madera, 8),
            new Ingredient(ItemRegistry.Piedra, 10)
        },
        amount: 1,
        requiresCraftingTable: false
    );

    public static readonly Recipe IronAxeRecipe = new Recipe(
        id: 5,
        result: ItemRegistry.HachaDeHierro,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.HachaDePiedra, 1),
            new Ingredient(ItemRegistry.Madera, 12),
            new Ingredient(ItemRegistry.Hierro, 10)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe IronPickaxeRecipe = new Recipe(
        id: 6,
        result: ItemRegistry.PicoDeHierro,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.PicoDePiedra, 1),
            new Ingredient(ItemRegistry.Madera, 15),
            new Ingredient(ItemRegistry.Hierro, 15)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe TitaniumAxeRecipe = new Recipe(
        id: 7,
        result: ItemRegistry.HachaDeTitanio,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.HachaDeHierro, 1),
            new Ingredient(ItemRegistry.Madera, 20),
            new Ingredient(ItemRegistry.Titanio, 15)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe TitaniumPickaxeRecipe = new Recipe(
        id: 8,
        result: ItemRegistry.PicoDeTitanio,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.PicoDeHierro, 1),
            new Ingredient(ItemRegistry.Madera, 20),
            new Ingredient(ItemRegistry.Titanio, 20)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe OxygenGeneratorRecipe = new Recipe(
        id: 9,
        result: ItemRegistry.GeneradorO2,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Chip, 3),
            new Ingredient(ItemRegistry.OxigenoEmbotellado, 2),
            new Ingredient(ItemRegistry.Hierro, 10),
            new Ingredient(ItemRegistry.Titanio, 1),
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe SiliconRecipe = new Recipe(
        id: 10,
        result: ItemRegistry.Silicio,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Arena, 1),
            new Ingredient(ItemRegistry.Piedra, 1)
        },
        amount: 2,
        requiresCraftingTable: false
    );

    public static readonly Recipe ChipRecipe = new Recipe(
        id: 11,
        result: ItemRegistry.Chip,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Silicio, 4),
            new Ingredient(ItemRegistry.Piedra, 1),
            new Ingredient(ItemRegistry.Hierro, 4)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe EmptyBottleRecipe = new Recipe(
        id: 12,
        result: ItemRegistry.BotellaVacia,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Hierro, 1)
        },
        amount: 1,
        requiresCraftingTable: false
    );

     public static readonly Recipe CraftingTableRecipe = new Recipe(
        id: 13,
        result: ItemRegistry.CraftingTable,
        ingredients: new List<Ingredient>
        {

            new Ingredient(ItemRegistry.Piedra, 5),
            new Ingredient(ItemRegistry.Hierro, 5),
            new Ingredient(ItemRegistry.lowCrystal, 2),
            new Ingredient(ItemRegistry.mediumCrystal, 1)

        },
        amount : 1,
        requiresCraftingTable: false
    );

    public static readonly Recipe LowArmorRecipe = new Recipe(
        id: 14,
        result: ItemRegistry.lowArmor,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Chip, 2),
            new Ingredient(ItemRegistry.Hierro, 10),
            new Ingredient(ItemRegistry.lowCrystal, 10),

        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe MidArmorRecipe = new Recipe(
        id: 15,
        result: ItemRegistry.mediumArmor,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.lowArmor, 1),
            new Ingredient(ItemRegistry.Chip, 2),
            new Ingredient(ItemRegistry.Titanio, 10),
            new Ingredient(ItemRegistry.mediumCrystal, 10),
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe AdvancedArmorRecipe = new Recipe(
        id: 16,
        result: ItemRegistry.advancedArmor,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.mediumArmor, 1),
            new Ingredient(ItemRegistry.Chip, 2),
            new Ingredient(ItemRegistry.Torio, 15),
            new Ingredient(ItemRegistry.advancedCrystal, 20),
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe WoodenWallRecipe = new Recipe(
        id: 17,
        result: ItemRegistry.WoodenWall,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 2)
        },
        amount: 1,
        requiresCraftingTable: false
    );

    public static readonly Recipe StoneWallRecipe = new Recipe(
        id: 18,
        result: ItemRegistry.StoneWall,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Piedra, 2)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe TurbineRecipe = new Recipe(
        id: 19,
        result: ItemRegistry.turbine,
        ingredients: new List<Ingredient>
        {

            new Ingredient(ItemRegistry.Hierro, 10),
            new Ingredient(ItemRegistry.Titanio, 10),
            new Ingredient(ItemRegistry.Torio, 5),
            new Ingredient(ItemRegistry.Silicio, 30)

        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe WingRecipe = new Recipe(
        id: 20,
        result: ItemRegistry.wing,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Hierro, 10),
            new Ingredient(ItemRegistry.Titanio, 15),
            new Ingredient(ItemRegistry.Silicio, 20),
            new Ingredient(ItemRegistry.lowCrystal, 15)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe FuelTankRecipe = new Recipe(
        id: 21,
        result: ItemRegistry.fuelTank,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Hierro, 30),
            new Ingredient(ItemRegistry.Torio, 5),
            new Ingredient(ItemRegistry.mediumCrystal, 20)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe CabinRecipe = new Recipe(
        id: 22,
        result: ItemRegistry.cabin,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Titanio, 25),
            new Ingredient(ItemRegistry.Chip, 5),
            new Ingredient(ItemRegistry.advancedCrystal, 10),
            new Ingredient(ItemRegistry.GeneradorO2, 1)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe ShipRecipe = new Recipe(
        id: 23,
        result: ItemRegistry.FinalShip,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.turbine, 1),
            new Ingredient(ItemRegistry.wing, 2),
            new Ingredient(ItemRegistry.fuelTank, 1),
            new Ingredient(ItemRegistry.cabin, 1)
        },
        amount: 1,
        requiresCraftingTable: true
    );

    public static readonly Recipe WoodenFloorRecipe = new Recipe(
        id: 24,
        result: ItemRegistry.WoodenFloor,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 2)
        },
        amount: 2,
        requiresCraftingTable: false
    );

    public static readonly IReadOnlyList<Recipe> All = new List<Recipe>
    {
        CraftingTableRecipe,
        WoodenAxeRecipe,
        WoodenPickaxeRecipe,
        EmptyBottleRecipe,
        StonePickaxeRecipe,
        StoneAxeRecipe,
        SiliconRecipe,
        ChipRecipe,

        WoodenFloorRecipe,
        WoodenWallRecipe,
        StoneWallRecipe,

        IronPickaxeRecipe,
        IronAxeRecipe,
        OxygenGeneratorRecipe,
        TitaniumAxeRecipe,
        TitaniumPickaxeRecipe,

        LowArmorRecipe,
        MidArmorRecipe,
        AdvancedArmorRecipe,

        TurbineRecipe,
        WingRecipe,
        FuelTankRecipe,
        CabinRecipe,
        ShipRecipe,

    };

     private static readonly Dictionary<int, Recipe> recipesById = new Dictionary<int, Recipe>();

    static RecipeRegistry()
    {
        foreach (Recipe recipe in All)
        {
            recipesById.Add(recipe.Id, recipe);
        }
    }

    public static bool TryGetById(int id, out Recipe? recipe)
    {
        return recipesById.TryGetValue(id, out recipe);
    }
}
