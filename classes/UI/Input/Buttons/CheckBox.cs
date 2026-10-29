using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace EXILION.UI;

public class CheckBox
{
    private readonly string text;
    private readonly SpriteFont font;
    private readonly Texture2D pixel;
    private readonly Point size;
    private MouseState previousMouse;
    private bool isHovered;
    private bool madeHoverSound;

    public Vector2 position;
    public bool IsChecked { get; private set; }

    public Rectangle bounds => new Rectangle((int)position.X, (int)position.Y, size.X, size.Y);

    public CheckBox(string text, Rectangle bounds, SpriteFont font, Texture2D pixel, bool isChecked)
    {
        this.text = text;
        this.position = new Vector2(bounds.X, bounds.Y);
        this.size = new Point(bounds.Width, bounds.Height);
        this.font = font;
        this.pixel = pixel;
        this.IsChecked = isChecked;
    }

    public bool Update(MouseState currentMouse)
    {
        isHovered = bounds.Contains(currentMouse.Position);
        if (isHovered && !madeHoverSound)
        {
            SFX.Play(Assets.SoundEffects.buttonHover);
            madeHoverSound = true;
        }
        else if (!isHovered)
        {
            madeHoverSound = false;
        }

        bool clicked = currentMouse.LeftButton == ButtonState.Released &&
                       previousMouse.LeftButton == ButtonState.Pressed &&
                       bounds.Contains(currentMouse.Position);
        previousMouse = currentMouse;

        if (clicked)
        {
            IsChecked = !IsChecked;
            SFX.Play(Assets.SoundEffects.buttonClick);
        }

        return clicked;
    }

    public void SetChecked(bool isChecked)
    {
        IsChecked = isChecked;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Rectangle controlBounds = bounds;
        Color textColor = isHovered ? Color.LightGray : Color.White;
        Vector2 textSize = font.MeasureString(text);
        Vector2 textPosition = new Vector2(
            controlBounds.X,
            controlBounds.Y + (controlBounds.Height - textSize.Y) / 2f
        );
        spriteBatch.DrawString(font, text, textPosition, textColor);

        int boxSize = Math.Min(controlBounds.Height / 2, controlBounds.Width / 4);
        int boxX = controlBounds.Right - boxSize;
        int boxY = controlBounds.Y + (controlBounds.Height - boxSize) / 2;
        Rectangle boxBounds = new Rectangle(boxX, boxY, boxSize, boxSize);
        Color borderTop = new Color(46, 15, 74);
        Color borderBottom = new Color(25, 15, 35);
        for (int y = 0; y < boxSize; y++)
        {
            float progress = boxSize <= 1 ? 0f : y / (float)(boxSize - 1);
            Color gradientColor = Color.Lerp(borderTop, borderBottom, progress);
            spriteBatch.Draw(pixel, new Rectangle(boxX, boxY + y, boxSize, 1), gradientColor);
        }

        int inset = Math.Max(1, boxSize / 6);
        Rectangle innerBounds = new Rectangle(
            boxBounds.X + inset,
            boxBounds.Y + inset,
            boxBounds.Width - inset * 2,
            boxBounds.Height - inset * 2
        );
        spriteBatch.Draw(pixel, innerBounds, IsChecked ? Color.White : new Color(25, 15, 35));
    }
}
