using System.Collections.Generic;

namespace EXILION.Items;

public static class RecipeRegistry
{

    public static readonly Recipe WoodenAxeRecipe = new Recipe(
        id: 1,
        result: ItemRegistry.HachaDeMadera,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 4)
        },
        amount: 1
    );
    public static readonly Recipe StoneAxeRecipe = new Recipe(
        id: 2,
        result: ItemRegistry.HachaDePiedra,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 6),
            new Ingredient(ItemRegistry.Piedra, 6)
        },
        amount: 1
    );

    public static readonly Recipe WoodenPickaxeRecipe = new Recipe(
        id: 3,
        result: ItemRegistry.PicoDeMadera,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 8)
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

    public static readonly IReadOnlyList<Recipe> All = new List<Recipe>
    {
        StoneAxeRecipe,
        WoodenAxeRecipe,
        WoodenPickaxeRecipe,
        StonePickaxeRecipe
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