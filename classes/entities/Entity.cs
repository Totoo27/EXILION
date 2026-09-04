using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Entities;
public abstract class Entity
{
    protected Sprite sprite { get; private set; }

    public Vector2 position;
    private Point hitboxSize;
    protected Color color = Color.White;

    protected Rectangle hitbox
    {
        get
        {
            return new Rectangle((int)position.X - hitboxSize.X / 2, (int)position.Y - hitboxSize.Y / 2, hitboxSize.X, hitboxSize.Y);
        }

        private set { }
    }

    protected GameContext gameContext;

    
    public Entity(Vector2 position, Sprite sprite, GameContext gameContext, int hitboxWidth = 40, int hitboxHeight = 40)
    {
        this.position = position;
        this.sprite = sprite;
        this.gameContext = gameContext;
        this.hitboxSize = new Point(gameContext.ScaleX(hitboxWidth), gameContext.ScaleY(hitboxHeight));
    }

    public Rectangle GetHitbox() => hitbox;

    public virtual void Update(Vector2 mousePosition)
    {
        
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {

        sprite.Draw(spriteBatch, color);
        if (gameContext.showHitboxes)
        {
            spriteBatch.Draw(pixel, hitbox, Color.Red);
        }
    }
}