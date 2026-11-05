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
        amount: 1
    );

    public static readonly Recipe WoodenPickaxeRecipe = new Recipe(
        id: 2,
        result: ItemRegistry.PicoDeMadera,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 8)
        },
        amount: 1
    );

    public static readonly Recipe StoneAxeRecipe = new Recipe(
        id: 3,
        result: ItemRegistry.HachaDePiedra,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 6),
            new Ingredient(ItemRegistry.Piedra, 6)
        },
        amount: 1
    );

    public static readonly Recipe StonePickaxeRecipe = new Recipe(
        id: 4,
        result: ItemRegistry.PicoDePiedra,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 8),
            new Ingredient(ItemRegistry.Piedra, 10)
        },
        amount: 1
    );

    public static readonly Recipe IronAxeRecipe = new Recipe(
        id: 5,
        result: ItemRegistry.HachaDeHierro,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 12),
            new Ingredient(ItemRegistry.Hierro, 10)
        },
        amount: 1
    );

    public static readonly Recipe IronPickaxeRecipe = new Recipe(
        id: 6,
        result: ItemRegistry.PicoDeHierro,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 15),
            new Ingredient(ItemRegistry.Hierro, 15)
        },
        amount: 1
    );

    public static readonly Recipe TitaniumAxeRecipe = new Recipe(
        id: 7,
        result: ItemRegistry.HachaDeTitanio,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 20),
            new Ingredient(ItemRegistry.Titanio, 15)
        },
        amount: 1
    );

    public static readonly Recipe TitaniumPickaxeRecipe = new Recipe(
        id: 8,
        result: ItemRegistry.PicoDeTitanio,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 20),
            new Ingredient(ItemRegistry.Titanio, 20)
        },
        amount: 1
    );

    public static readonly Recipe OxygenGeneratorRecipe = new Recipe(
        id: 9,
        result: ItemRegistry.GeneradorO2,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 4),
            new Ingredient(ItemRegistry.Piedra, 3)
        },
        amount: 1
    );

    public static readonly IReadOnlyList<Recipe> All = new List<Recipe>
    {
        WoodenAxeRecipe,
        WoodenPickaxeRecipe,
        StonePickaxeRecipe,
        StoneAxeRecipe,
        IronPickaxeRecipe,
        IronAxeRecipe,
        TitaniumAxeRecipe,
        TitaniumPickaxeRecipe,
        OxygenGeneratorRecipe
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
