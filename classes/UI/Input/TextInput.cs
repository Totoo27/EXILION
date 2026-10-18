using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.UI;

public sealed class TextInput
{
    private readonly Func<string, char, bool> canAppend;

    public TextInput(int maxLength, Func<string, char, bool> canAppend, string placeholder = "")
    {
        MaxLength = maxLength;
        this.canAppend = canAppend;
        Placeholder = placeholder;
    }

    public string Text { get; private set; } = string.Empty;
    public string Placeholder { get; }
    public int MaxLength { get; }
    public Rectangle Bounds { get; set; }
    public bool IsFocused { get; set; }

    public bool Contains(Point point) => Bounds.Contains(point);

    public bool TryAppend(char character)
    {
        if (Text.Length >= MaxLength) return false;
        if (!canAppend(Text, character)) return false;

        Text += character;
        return true;
    }

    public bool Backspace()
    {
        if (Text.Length == 0) return false;

        Text = Text[..^1];
        return true;
    }

    public void Clear() => Text = string.Empty;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font, float textScale = 1f)
    {
        const int border = 2;
        const int padding = 10;

        Color background = IsFocused ? new Color(40, 25, 60) : new Color(15, 10, 25);
        Color borderColor = IsFocused ? new Color(170, 110, 255) : new Color(46, 15, 74);

        spriteBatch.Draw(pixel, Bounds, background);

        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, border), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Bottom - border, Bounds.Width, border), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.X, Bounds.Y, border, Bounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(Bounds.Right - border, Bounds.Y, border, Bounds.Height), borderColor);

        Vector2 textPosition = new(Bounds.X + padding, Bounds.Center.Y - font.LineSpacing * textScale / 2f);

        if (Text.Length == 0 && !IsFocused)
        {
            spriteBatch.DrawString(font, Placeholder, textPosition, Color.Gray, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }
        else
        {
            spriteBatch.DrawString(font, Text, textPosition, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        if (IsFocused && IsCaretVisible)
        {
            float caretX = textPosition.X + font.MeasureString(Text).X * textScale + 2;

            spriteBatch.Draw(
                pixel,
                new Rectangle((int)caretX, Bounds.Y + padding, 2, Bounds.Height - padding * 2),
                Color.White
            );
        }
    }

    private static bool IsCaretVisible => (Environment.TickCount / 500) % 2 == 0;
}
