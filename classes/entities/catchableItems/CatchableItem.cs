using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;
using EXILION.Saving;
using System;

namespace EXILION.Entities.CatchableItems;

public class CatchableItem : Entity
{
    private const int LifeTime = 20; // Seconds
    private float lifeTimer = 0;

    private readonly EXILION.Client.CatchableItemPulse pulse = new();
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

        lifeTimer += deltaTime;
        if (lifeTimer >= LifeTime)
        {
            MarkPicked();
            return;
        }

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
