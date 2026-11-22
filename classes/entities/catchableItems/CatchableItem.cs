using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;
using EXILION.Saving;
using System;

namespace EXILION.Entities.CatchableItems;

public class CatchableItem : Entity
{
    private readonly EXILION.Client.CatchableItemPulse pulse = new();
    private const int HitboxSize = 28; // más chico que el del jugador (40)

    private readonly EXILION.Logic.CatchableItemState state;
    public ItemStack Stack => state.Stack;
    public bool Picked => state.Picked;

    public CatchableItem(ItemStack stack, Vector2 position, Sprite sprite, GameContext gameContext)
        : base(position, sprite, gameContext, HitboxSize, HitboxSize)
    {
        state = new EXILION.Logic.CatchableItemState(stack);

        sprite.Update(0f, position);
    }

    public void MarkPicked() => state.MarkPicked();

    public void Update(float deltaTime)
    {
        state.Update(deltaTime);
        if (Picked) return;

        pulse.Update(deltaTime);
        sprite.SetScaleMultiplier(pulse.Scale);
    }

    public new void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (Picked) return;
        base.Draw(spriteBatch, pixel);
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
