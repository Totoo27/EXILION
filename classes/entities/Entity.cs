using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;
public class Entity
{
    public Vector2 position;
    public Sprite sprite { get; private set;}

    private Point hitboxSize = new Point(40, 40);

    protected Rectangle hitbox
    {
        get
        {
            return new Rectangle((int)position.X - hitboxSize.X / 2, (int)position.Y - hitboxSize.Y / 2, hitboxSize.X, hitboxSize.Y);
        }
    }

    public Entity(Vector2 position, Sprite sprite)
    {
        this.position = position;
        this.sprite = sprite;

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
        spriteBatch.Draw(pixel, hitbox, Color.Red);

    }

}