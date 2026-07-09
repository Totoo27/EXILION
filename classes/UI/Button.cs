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

    public void Update(MouseState currentMouse)
    {
        IsHovered = Bounds.Contains(currentMouse.Position);

        if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released && this.Bounds.Contains(currentMouse.Position))
        {
            Console.WriteLine("Click!");
        }
        previousMouse = currentMouse;

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Color color = this.IsHovered ? Color.LightGray : Color.White;

        spriteBatch.Draw(Texture, Bounds, color);
    }
}