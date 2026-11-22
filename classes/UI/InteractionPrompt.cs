
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EXILION.UI;

public class InteractionPrompt
{
    private readonly SpriteFont font;
    private readonly float scale;

    public bool Visible { get; set; }

    public InteractionPrompt(SpriteFont font, float scale)
    {
        this.font = font;
        this.scale = scale;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, Rectangle targetHitbox)
    {
        if (!Visible) return;

        int width = (int)(42 * scale);
        int height = (int)(34 * scale);
        int border = Math.Max(1, (int)(2 * scale));

        int x = targetHitbox.Center.X - width / 2;
        int y = targetHitbox.Top - height - (int)(10 * scale);

        Rectangle background = new Rectangle(x, y, width, height);

        // Background
        spriteBatch.Draw(pixel, background, Color.Black * 0.65f);

        // Border
        spriteBatch.Draw(pixel,
            new Rectangle(x, y, width, border), Color.White * 0.85f);

        spriteBatch.Draw(pixel,
            new Rectangle(x, y + height - border, width, border), Color.White * 0.85f);

        spriteBatch.Draw(pixel,
            new Rectangle(x, y, border, height), Color.White * 0.85f);

        spriteBatch.Draw(pixel,
            new Rectangle(x + width - border, y, border, height), Color.White * 0.85f);

        // Key text
        string key = "E";
        Vector2 textSize = font.MeasureString(key) * scale;

        Vector2 textPosition = new Vector2(
            x + (width - textSize.X) / 2,
            y + (height - textSize.Y) / 2
        );

        spriteBatch.DrawString(font, key, textPosition, Color.White, 0f,
            Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
