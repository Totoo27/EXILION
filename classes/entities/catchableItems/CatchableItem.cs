using Microsoft.Xna.Framework;
using EXILION.Items;
using EXILION.Saving;
using System;

namespace EXILION.Entities.CatchableItems;

public class CatchableItem : Entity
{
    private const int HitboxSize = 28; // más chico que el del jugador (40)

    private readonly EXILION.Logic.CatchableItemState state;
    private Vector2 networkTargetPosition;
    private Vector2 networkVisualPosition;
    private bool hasNetworkTarget;
    public Guid NetworkId { get; }
    public Vector2 VisualPosition => hasNetworkTarget ? networkVisualPosition : position;
    public ItemStack Stack => state.Stack;
    public bool Picked => state.Picked;

    public CatchableItem(ItemStack stack, Vector2 position, Point hitboxSize, Guid? networkId = null)
        : base(position, hitboxSize)
    {
        state = new EXILION.Logic.CatchableItemState(stack);
        NetworkId = networkId ?? Guid.NewGuid();
        networkVisualPosition = position;

    }

    public void MarkPicked() => state.MarkPicked();

    public void ApplyNetworkPosition(Vector2 targetPosition)
    {
        if (!hasNetworkTarget)
            networkVisualPosition = position;
        networkTargetPosition = targetPosition;
        hasNetworkTarget = true;
        position = targetPosition;
    }

    public void UpdateNetworkVisual(float deltaTime)
    {
        if (!hasNetworkTarget) return;
        float amount = 1f - MathF.Exp(-24f * deltaTime);
        networkVisualPosition = Vector2.Lerp(networkVisualPosition, networkTargetPosition, amount);
    }

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
