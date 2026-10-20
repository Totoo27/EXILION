using System;

namespace EXILION.Items;

public class ItemStack
{
    public Item Item { get; }
    public int Quantity { get; private set; }

    public ItemStack(Item item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }


    public bool IsFull => Quantity >= Item.GetMaxStackSize();

    public int SpaceLeft => Item.GetMaxStackSize() - Quantity;

    public int Add(int amount)
    {
        int spaceLeft = SpaceLeft;
        int toAdd = Math.Min(amount, spaceLeft);
        Quantity += toAdd;
        return amount - toAdd;
    }

     public int Remove(int amount)
    {
        int toRemove = Math.Min(amount, Quantity);
        Quantity -= toRemove;
        return toRemove;
    }

    public bool CanStackWith(Item other)
    {
        return Item.Id == other.Id && !IsFull;
    }

}