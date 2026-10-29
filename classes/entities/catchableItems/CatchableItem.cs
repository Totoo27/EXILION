using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;
using EXILION.Saving;
using System;

namespace EXILION.Entities.CatchableItems;

public class CatchableItem : Entity
{
    private const float PulseSpeed = 1.5f;
    private const float PulseAmount = 0.12f;
    private float animationTime;
    private const int HitboxSize = 28; // más chico que el del jugador (40)

    public ItemStack Stack { get; }
    public bool Picked { get; private set; }

    public CatchableItem(ItemStack stack, Vector2 position, Sprite sprite, GameContext gameContext)
        : base(position, sprite, gameContext, HitboxSize, HitboxSize)
    {
        Stack = stack;

        sprite.Update(0f, position);
    }

    public void MarkPicked() => Picked = true;

    public void Update(float deltaTime)
    {
        if (Picked) return;

        animationTime += deltaTime;
        float pulse = 1f + MathF.Sin(animationTime * PulseSpeed) * PulseAmount;
        sprite.SetScaleMultiplier(pulse);
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
