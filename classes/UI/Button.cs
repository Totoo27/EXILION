using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;
public class Button
{
    private Texture2D texture;
    private string text;

    // Button bounds
    public Vector2 position;
    private Point size;
    public Rectangle bounds
    {
        get
        {
            return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y);
        }

        private set{}
    }

    private MouseState previousMouse;

    private bool isHovered = false;
    private bool madeSound = false;

    private SpriteFont font;

    public Button(String text, Rectangle bounds, Texture2D texture, SpriteFont font)
    {
        this.text = text;
        this.position = new Vector2(bounds.X, bounds.Y);
        this.size = new Point(bounds.Width, bounds.Height);
        this.texture = texture;
        this.font = font;
    }

    public bool isClicked(MouseState currentMouse)
    {
        bool clicked = false;
        isHovered = bounds.Contains(currentMouse.Position);

        if (isHovered && !madeSound)
        {
            Assets.SoundEffects.buttonHover.Play();
            madeSound = true;

        } else if(!isHovered)
        {
            madeSound = false;
        }

        if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed && this.bounds.Contains(currentMouse.Position))
        {
            clicked = true;
        }

        previousMouse = currentMouse;

        return clicked;

    }


    public void Draw(SpriteBatch spriteBatch, float opacity)
    {

        if(opacity <= 0) return;

        Color color = this.isHovered ? Color.LightGray : Color.White;
        Vector2 textSize = font.MeasureString(text);
        Vector2 textPosition = new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + (bounds.Height - textSize.Y) / 2);

        spriteBatch.Draw(texture, bounds, color * opacity);
        spriteBatch.DrawString(
            font,
            text,
            textPosition,
            color * opacity);

    }
    public void Draw(SpriteBatch spriteBatch)
    {
        this.Draw(spriteBatch, 1f);
    }
}