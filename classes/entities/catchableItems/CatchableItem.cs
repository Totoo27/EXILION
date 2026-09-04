using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;

namespace EXILION.Entities.CatchableItems;

public class CatchableItem : Entity
{
    private const int HitboxSize = 24; // más chico que el del jugador (40)

    public ItemStack Stack { get; }
    public bool Picked { get; private set; }

    public CatchableItem(ItemStack stack, Vector2 position, Sprite sprite, GameContext gameContext)
        : base(position, sprite, gameContext, HitboxSize, HitboxSize)
    {
        Stack = stack;

        sprite.Update(0f, position);
    }

    public void MarkPicked() => Picked = true;

    public new void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (Picked) return;
        base.Draw(spriteBatch, pixel);
    }
}