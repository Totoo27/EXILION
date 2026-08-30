using System.Collections.Generic;
using EXILION.Items;

namespace EXILION.Entities.LivingThings;

public class Inventory
{
     public const int DefaultStacksAmount = 24;

     public int Capacity { get; private set; }

     private readonly List<ItemStack> stacks;

    public Inventory(int capacity = DefaultStacksAmount)
    {
        Capacity = capacity;
        stacks = new List<ItemStack>();
    }

     public int AddItem(Item item, int amount)
    {
        foreach (var stack in stacks)
        {
            if (amount <= 0) break;
            if (stack.CanStackWith(item))
            {
                amount = stack.Add(amount);
            }
        }

        while (amount > 0 && stacks.Count < Capacity)
        {
            var newStack = new ItemStack(item, 0);
            amount = newStack.Add(amount);
            stacks.Add(newStack);
        }

        return amount;
    }

    public int RemoveItem(Item item, int amount)
    {
        int removed = 0;

        for (int i = stacks.Count - 1; i >= 0 && amount > 0; i--)
        {
            var stack = stacks[i];
            if (stack.Item.Id != item.Id) continue;

            int taken = stack.Remove(amount);
            removed += taken;
            amount -= taken;

            if (stack.Quantity == 0)
                stacks.RemoveAt(i);
        }

        return removed;
    }

    public int GetItemCount(Item item)
    {
        int total = 0;
        foreach (var stack in stacks)
        {
            if (stack.Item.Id == item.Id)
                total += stack.Quantity;
        }
        return total;
    }

    public bool HasItem(Item item, int amount)
    {
        return GetItemCount(item) >= amount;
    }

    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= stacks.Count) return null;
        return stacks[index];
    }


}