using System.Collections.Generic;
using EXILION.Items;

namespace EXILION.Entities.LivingThings;

public class Inventory
{
     public const int DefaultStacksAmount = 24;

     public int Capacity { get; private set; }

     private readonly ItemStack[] slots;

    public Inventory(int capacity = DefaultStacksAmount)
    {
        Capacity = capacity;
        slots = new ItemStack[capacity];
    }

     public int AddItem(Item item, int amount)
    {
        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i] != null && slots[i].CanStackWith(item))
            {
                amount = slots[i].Add(amount);
            }
        }

        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i] == null)
            {
                var newStack = new ItemStack(item, 0);
                amount = newStack.Add(amount);
                slots[i] = newStack;
            }
        }

        return amount;
    }

     public int RemoveItem(Item item, int amount)
    {
        int removed = 0;

        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i] == null || slots[i].Item.Id != item.Id) continue;

            int taken = slots[i].Remove(amount);
            removed += taken;
            amount -= taken;

            if (slots[i].Quantity == 0)
                slots[i] = null;
        }

        return removed;
    }

     public int GetItemCount(Item item)
    {
        int total = 0;
        foreach (var stack in slots)
        {
            if (stack != null && stack.Item.Id == item.Id)
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
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }


    public void MoveItem(int from, int to)
    {
        if (from < 0 || from >= slots.Length) return;
        if (to < 0 || to >= slots.Length) return;
        if (from == to) return;

        ItemStack source = slots[from];
        if (source == null) return; 

        ItemStack destination = slots[to];

        if (destination == null)
        {
            slots[to] = source;
            slots[from] = null;
            return;
        }

        if (destination.Item.Id == source.Item.Id)
        {
            int leftover = destination.Add(source.Quantity);

            slots[from] = leftover == 0
                ? null
                : new ItemStack(source.Item, leftover);

            return;
        }

        
        slots[from] = destination;
        slots[to] = source;
    }

    


}