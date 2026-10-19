using System.Collections.Generic;

namespace EXILION.Items;

public static class RecipeRegistry
{
    public static readonly Recipe AxeRecipe = new Recipe(
        id: 1,
        result: ItemRegistry.HachaTest,
        ingredients: new List<Ingredient>
        {
            new Ingredient(ItemRegistry.Madera, 2),
            new Ingredient(ItemRegistry.Piedra, 3)
        },
        amount: 1
    );

    public static readonly IReadOnlyList<Recipe> All = new List<Recipe>
    {
        AxeRecipe
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