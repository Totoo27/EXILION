using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using EXILION.Entities.LivingThings;
using System;

namespace EXILION.UI.Bar;

public class LinearBar : Bar
{
    
    private Rectangle fillRectangle;
    private Rectangle positionRectangle;
    private Rectangle sourceRectangle;
    private bool vertical;

    public LinearBar(Texture2D background, Texture2D progress, Texture2D setoff, Rectangle rectangle, Rectangle positionRectangle, bool vertical, int maxValue, Vector2 textPosition)
    : base(background, progress, setoff, rectangle, maxValue, textPosition)
    {
        this.positionRectangle = positionRectangle;
        fillRectangle = new Rectangle(positionRectangle.X, positionRectangle.Y, positionRectangle.Width, positionRectangle.Height);
        this.vertical = vertical;
        setValue(maxValue);
    }

    public LinearBar(Texture2D background, Texture2D progress, Rectangle rectangle, Rectangle positionRectangle, bool vertical,  int maxValue, Vector2 textPosition)
    : base(background, progress, null, rectangle, maxValue, textPosition)
    {

        this.positionRectangle = positionRectangle;
        fillRectangle = new Rectangle(positionRectangle.X, positionRectangle.Y, positionRectangle.Width, positionRectangle.Height);
        this.vertical = vertical;
        setValue(maxValue);
        
    }

    public LinearBar(Texture2D background, Texture2D progress, Texture2D setoff, Rectangle rectangle, Rectangle positionRectangle, bool vertical,  int maxValue)
    : base(background, progress, setoff, rectangle, maxValue, Vector2.Zero)
    {
        this.positionRectangle = positionRectangle;
        fillRectangle = new Rectangle(positionRectangle.X, positionRectangle.Y, positionRectangle.Width, positionRectangle.Height);
        this.vertical = vertical;
        setValue(maxValue);
    }

    public override void setValue(int value)
    {
        int clampedValue = Math.Clamp(value, 0, maxValue);
        float ratio = (float)clampedValue / maxValue;

        if (vertical)
        {
            int newHeight = (int)(positionRectangle.Height * ratio);
            
            fillRectangle.Height = newHeight;
            fillRectangle.Y = positionRectangle.Y + positionRectangle.Height - newHeight;

            int sourceHeight = (int)(progress.Height * ratio);
            int sourceY = progress.Height - sourceHeight;
            sourceRectangle = new Rectangle(0, sourceY, progress.Width, sourceHeight);
        }
        else
        {
            int newWidth = (int)(positionRectangle.Width * ratio);

            fillRectangle.Width = newWidth;

            int sourceWidth = (int)(progress.Width * ratio);
            sourceRectangle = new Rectangle(0, 0, sourceWidth, progress.Height);
        }

        int percentage = (int)(ratio * 100);
        setTextPercentage(percentage);
    }

    protected override void drawProgress(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(progress, fillRectangle, sourceRectangle, progressColor);
    }

}