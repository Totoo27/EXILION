using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;
public class Button
{
    private Rectangle bounds;
    private Texture2D texture;
    private string text;

    private MouseState previousMouse;

    private bool IsHovered;

    private SpriteFont font;

    public Button(String text, Rectangle bounds, Texture2D texture, SpriteFont font)
    {
        this.text = text;
        this.bounds = bounds;
        this.texture = texture;
        this.font = font;
    }

    public bool isClicked(MouseState currentMouse)
    {
        IsHovered = bounds.Contains(currentMouse.Position);

        if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed && this.bounds.Contains(currentMouse.Position))
        {
            return true;
        }
        previousMouse = currentMouse;

        return false;

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = this.IsHovered ? Color.LightGray : Color.White;
        Vector2 textSize = font.MeasureString(text);
        Vector2 textPosition = new Vector2(bounds.X + (bounds.Width - textSize.X) / 2, bounds.Y + (bounds.Height - textSize.Y) / 2);

        spriteBatch.Draw(texture, bounds, color);
        spriteBatch.DrawString(
            font,
            text,
            textPosition,
            color);

    }
}