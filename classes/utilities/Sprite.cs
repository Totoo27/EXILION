using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;

public class Sprite
{
    
    private Vector2 position;
    private Texture2D texture;
    private float rotation;
    private float scale;
    private Rectangle? sourceRectangle;
    private readonly float baseScale;

    public Sprite(Texture2D texture, float scale, Rectangle? sourceRectangle = null)
    {
        this.texture = texture;
        this.scale = scale;
        this.sourceRectangle = sourceRectangle;
        this.baseScale = scale;
    }

    public void SetScaleMultiplier(float multiplier)
    {
        scale = baseScale * multiplier;
    }

    public void Update(float angle, Vector2 position)
    {
        this.position = position;
        this.rotation = angle;
    }

    public void Draw(SpriteBatch spriteBatch, Color color)
    {

        spriteBatch.Draw(
            texture,
            position,
            sourceRectangle,
            color,
            rotation,
            new Vector2((sourceRectangle?.Width ?? texture.Width) / 2f,
                (sourceRectangle?.Height ?? texture.Height) / 2f),
            scale,
            SpriteEffects.None,
            0f
        );
        
    }

}
