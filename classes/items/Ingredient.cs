namespace EXILION.Items;

public class Ingredient
{
    public Item Item {get; private set;}
    public int Quantity {get; private set;}

    public Ingredient(Item item, int quantity)
    {
        this.Item = item;
        this.Quantity = quantity;
    }
}