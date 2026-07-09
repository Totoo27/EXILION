using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;
public class Button
{
    private Rectangle Bounds;
    private Texture2D Texture;
    private string Text;

    private MouseState previousMouse;

    private bool IsHovered;

    public Button(String text, Rectangle bounds, Texture2D texture)
    {
        this.Text = text;
        this.Bounds = bounds;
        this.Texture = texture;
    }

    public bool isClicked(MouseState currentMouse)
    {
        IsHovered = Bounds.Contains(currentMouse.Position);

        if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed && this.Bounds.Contains(currentMouse.Position))
        {
            return true;
        }
        previousMouse = currentMouse;

        return false;

    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        Color color = this.IsHovered ? Color.LightGray : Color.White;
        Vector2 textSize = font.MeasureString(Text);
        Vector2 textPosition = new Vector2(Bounds.X + (Bounds.Width - textSize.X) / 2, Bounds.Y + (Bounds.Height - textSize.Y) / 2);

        spriteBatch.Draw(Texture, Bounds, color);
        spriteBatch.DrawString(
            font,
            Text,
            textPosition,
            color);

    }
}