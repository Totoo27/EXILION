using Microsoft.Xna.Framework;
using EXILION.Items;
using EXILION.Saving;
using System;

namespace EXILION.Entities.CatchableItems;

public class CatchableItem : Entity
{
    private const int HitboxSize = 28; // más chico que el del jugador (40)

    private readonly EXILION.Logic.CatchableItemState state;
    public ItemStack Stack => state.Stack;
    public bool Picked => state.Picked;

    public CatchableItem(ItemStack stack, Vector2 position, Point hitboxSize)
        : base(position, hitboxSize)
    {
        state = new EXILION.Logic.CatchableItemState(stack);

    }

    public void MarkPicked() => state.MarkPicked();

    public void Update(float deltaTime)
    {
        state.Update(deltaTime);
        if (Picked) return;

    }

    public CatchableItemSaveData GetSaveData()
    {
        return new CatchableItemSaveData
        {
            PositionX = position.X,
            PositionY = position.Y,
            ItemId = Stack.Item.Id,
            Quantity = Stack.Quantity
        };
    }
}
