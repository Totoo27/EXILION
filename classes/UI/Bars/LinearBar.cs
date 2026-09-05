using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using EXILION.Entities.LivingThings;
using System;

namespace EXILION.UI.Bar;

public class LinearBar : Bar
{
    
    private Rectangle fillRectangle;
    private Rectangle positionRectangle;
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

    public LinearBar(Texture2D background, Texture2D progress, Rectangle rectangle, Rectangle positionRectangle, bool vertical,  int maxValue)
    : base(background, progress, null, rectangle, maxValue, Vector2.Zero)
    {
        this.positionRectangle = positionRectangle;
        fillRectangle = new Rectangle(positionRectangle.X, positionRectangle.Y, positionRectangle.Width, positionRectangle.Height);
        this.vertical = vertical;
        setValue(maxValue);
    }

    public override void setValue(int value)
    {
        int percentage = value * 100 / maxValue;

        if (vertical)
        {
            fillRectangle.Height = positionRectangle.Height * percentage / 100;

            fillRectangle.Y = positionRectangle.Y + positionRectangle.Height - fillRectangle.Height;
        }
        else
        {
            fillRectangle.Width = positionRectangle.Width * percentage / 100;
        }

        setTextPercentage(Math.Clamp(percentage, 0, 100));
    }

    protected override void drawProgress(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(progress, fillRectangle, null, progressColor);
    }

}