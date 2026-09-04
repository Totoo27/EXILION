using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace EXILION.Entities;
public class Entity
{
    public Vector2 position;
    protected Sprite sprite { get; private set;}

    private Point hitboxSize;

    protected Rectangle hitbox
    {
        get
        {
            return new Rectangle((int)position.X - hitboxSize.X / 2, (int)position.Y - hitboxSize.Y / 2, hitboxSize.X, hitboxSize.Y);
        }
    }

    protected GameContext gameContext;

    public Entity(Vector2 position, Sprite sprite, GameContext gameContext)
    {
        this.position = position;
        this.sprite = sprite;
        this.gameContext = gameContext;
        this.hitboxSize = new Point(gameContext.ScaleX(40), gameContext.ScaleY(40));
    }

    public void Update(Vector2 mousePosition)
    {
        Vector2 direction = mousePosition - position;
        float angle = System.MathF.Atan2(direction.Y, direction.X);
        sprite.Update(angle, position);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {

        sprite.Draw(spriteBatch);
        if (gameContext.showHitboxes)
        {
            spriteBatch.Draw(pixel, hitbox, Color.Red);
        }

    }

}