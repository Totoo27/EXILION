using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;

public class Sprite
{
    
    public Texture2D texture;
    public Vector2 position;
    public float rotation;
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

    public void Update(Vector2 mousePosition)
    {
        Vector2 direction = mousePosition - position;
        rotation = System.MathF.Atan2(direction.Y, direction.X);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture,
            position,
            null,
            Color.White,
            rotation,
            new Vector2(texture.Width / 2f, texture.Height / 2f),
            1f,
            SpriteEffects.None,
            0f
        );
        
    }

}