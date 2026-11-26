using System.Collections.Generic;
using EXILION.Items;
using EXILION.Saving;
using System;

namespace EXILION.Entities.LivingThings;

public class Inventory
{
    #nullable enable
    public const int DefaultStacksAmount = 24;

    public event Action? UpdateInventoryUI;
    public int Capacity { get; private set; }

    private readonly ItemStack[] slots;
    public Armor? EquippedArmor { get; private set; }

    public Inventory(int capacity = DefaultStacksAmount)
    {
        Capacity = capacity;
        slots = new ItemStack[capacity];
    }

    public void Restore(List<InventorySlotSaveData> savedSlots)
    {
        Array.Clear(slots, 0, slots.Length);

        foreach (var savedSlot in savedSlots)
        {

            if (savedSlot.Slot < 0 || savedSlot.Slot >= slots.Length) continue;
            if (savedSlot.Quantity <= 0) continue;

            Item? item = ItemRegistry.GetById(savedSlot.ItemId);

            if (item == null) continue;
            slots[savedSlot.Slot] = new ItemStack(item, savedSlot.Quantity);
        }

        UpdateInventoryUI?.Invoke();
    }

    public void RestoreArmor(int? itemId)
    {
        Item? item = itemId.HasValue ? ItemRegistry.GetById(itemId.Value) : null;
        EquippedArmor = item as Armor;
        UpdateInventoryUI?.Invoke();
    }

    public bool EquipArmorFromSlot(int index)
    {
        if (index < 0 || index >= slots.Length || slots[index]?.Item is not Armor newArmor)
            return false;

        ItemStack source = slots[index];
        if (source.Quantity > 1 && EquippedArmor != null && !CanFit(EquippedArmor, 1))
        {
            return false;
        }

        source.Remove(1);
        if (source.Quantity == 0) slots[index] = null;

        Armor? replacedArmor = EquippedArmor;
        EquippedArmor = newArmor;
        if (replacedArmor != null) AddItem(replacedArmor, 1);
        UpdateInventoryUI?.Invoke();
        return true;
    }

    public ItemStack? TakeEquippedArmor()
    {
        if (EquippedArmor == null) return null;
        ItemStack stack = new(EquippedArmor, 1);
        EquippedArmor = null;
        UpdateInventoryUI?.Invoke();
        return stack;
    }

    public bool MoveEquippedArmorToSlot(int index)
    {
        if (EquippedArmor == null || index < 0 || index >= slots.Length) return false;
        if (slots[index]?.Item is Armor armorInSlot)
        {
            Armor armorToEquip = EquippedArmor;
            slots[index] = new ItemStack(armorToEquip, 1);
            EquippedArmor = armorInSlot;
            UpdateInventoryUI?.Invoke();
            return true;
        }

        if (slots[index] == null)
        {
            slots[index] = new ItemStack(EquippedArmor, 1);
        }
        else if (slots[index].CanStackWith(EquippedArmor))
        {
            if (slots[index].Add(1) != 0) return false;
        }
        else
        {
            return false;
        }
        EquippedArmor = null;
        UpdateInventoryUI?.Invoke();
        return true;
    }

    public List<InventorySlotSaveData> GetSaveData()
    {
        List<InventorySlotSaveData> saveData = new List<InventorySlotSaveData>();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                saveData.Add(new InventorySlotSaveData
                {
                    Slot = i,
                    ItemId = slots[i].Item.Id,
                    Quantity = slots[i].Quantity
                });
            }
        }

        return saveData;
    }

    public ItemStack? TakeStack(int index)
    {
        if (index < 0 || index >= slots.Length) return null;

        ItemStack? stack = slots[index];
        if (stack == null) return null;

        slots[index] = null;
        UpdateInventoryUI?.Invoke();

        return stack;
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

        UpdateInventoryUI?.Invoke();
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

            if (slots[i].Quantity == 0) slots[i] = null;
        }

        UpdateInventoryUI?.Invoke();
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

    public bool CanFit(Item item, int amount)
    {
        if (amount <= 0) return true;

        int space = 0;

        foreach (ItemStack stack in slots)
        {
            if (stack == null)
            {
                space += item.GetMaxStackSize();
            }
            else if (stack.CanStackWith(item))
            {
                space += stack.SpaceLeft;
            }

            if (space >= amount) return true;
        }

        return false;
    }

    public ItemStack? GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    public bool SplitStack(int index)
    {
        if (index < 0 || index >= slots.Length) return false;
        ItemStack? source = slots[index];
        if (source == null || source.Quantity < 2) return false;

        int emptySlot = Array.FindIndex(slots, stack => stack == null);
        if (emptySlot < 0) return false;

        int splitQuantity = source.Quantity / 2;
        source.Remove(splitQuantity);
        slots[emptySlot] = new ItemStack(source.Item, splitQuantity);
        UpdateInventoryUI?.Invoke();
        return true;
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
            UpdateInventoryUI?.Invoke();
            return;
        }

        if (destination.Item.Id == source.Item.Id)
        {
            int leftover = destination.Add(source.Quantity);

            slots[from] = leftover == 0 ? null : new ItemStack(source.Item, leftover);
            UpdateInventoryUI?.Invoke();
            return;
        }
        
        slots[from] = destination;
        slots[to] = source;
        UpdateInventoryUI?.Invoke();
    }


}
