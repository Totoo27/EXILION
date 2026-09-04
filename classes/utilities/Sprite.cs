using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;

public class Sprite
{
    
    private Vector2 position;
    private Texture2D texture;
    private float rotation;
    private float scale;

    public Sprite(Texture2D texture, float scale)
    {
        this.texture = texture;
        this.scale = scale;
    }

    public void Update(float angle, Vector2 position)
    {
        this.position = position;
        this.rotation = angle;
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
            scale,
            SpriteEffects.None,
            0f
        );
        
    }

}