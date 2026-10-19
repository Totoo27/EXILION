using System;
using System.Collections.Generic;

namespace EXILION.Items;

public class Recipe
{
    public int Id {get; private set;}
    public Item Result {get; private set;}
    public IReadOnlyList<Ingredient> Ingredients {get; private set;}
    public int Amount {get; private set;}

    public Recipe(int id,Item result, List<Ingredient> ingredients,int amount)
    {   

        if (ingredients == null || ingredients.Count == 0)
            throw new ArgumentException("A recipe needs at least one ingredient.", nameof(ingredients));

        if (amount <= 0)
            throw new ArgumentException("Amount must be a positive integer.", nameof(amount));

        this.Id = id;
        this.Result = result;
        this.Ingredients = ingredients;
        this.Amount = amount;
    }


    
}