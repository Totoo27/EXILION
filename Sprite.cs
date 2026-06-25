using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;

public class Sprite
{
    
    public Texture2D texture;
    public Vector2 position;
    public float speed;

    public Rectangle rectangle
    {
        get
        {
            return new Rectangle((int)position.X, (int)position.Y, 100, 100);
        }
    }

    public Sprite(Texture2D texture, Vector2 position, float speed)
    {
        this.texture = texture;
        this.position = position;
        this.speed = speed;
    }

}