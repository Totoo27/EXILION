using Microsoft.Xna.Framework;

namespace EXILION.Entities;
public abstract class Entity
{
    public Vector2 position;
    private Point hitboxSize;

    protected Rectangle hitbox
    {
        get
        {
            return new Rectangle((int)position.X - hitboxSize.X / 2, (int)position.Y - hitboxSize.Y / 2, hitboxSize.X, hitboxSize.Y);
        }

        private set { }
    }

    public Entity(Vector2 position, Point hitboxSize)
    {
        this.position = position;
        this.hitboxSize = hitboxSize;
    }

    public Rectangle GetHitbox() => hitbox;

    public virtual void Update(Vector2 mousePosition)
    {
        
    }

}
