using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using EXILION.Entities.LivingThings;
using System;

namespace EXILION.UI.Bar;

public abstract class Bar
{

    protected Texture2D background;
    protected Texture2D setoff;
    protected Texture2D progress;
    protected int percentage = 100;
    protected int maxValue;
    protected Rectangle rectangle;
    protected Color progressColor = Color.White;


    protected Color fontColor = Color.White;    
    private bool showText = true;
    private String text;
    private Vector2 textInnerPosition;
    private SpriteFont font = Assets.Fonts.PixelArtSmall;
    private Vector2 textPosition;
    public Bar(Texture2D background, Texture2D progress, Texture2D setoff, Rectangle rectangle, int maxValue, Vector2 textPosition)
    {

        if (maxValue <= 0)
        {
            throw new ArgumentException("The max value must be above 0");
        }

        if(progress == null)
        {
            throw new ArgumentException("The progress texture can't be null");   
        }

        this.background = background;
        this.setoff = setoff;
        this.progress = progress;
        this.rectangle = rectangle;
        this.maxValue = maxValue;

        SpriteFont font = Assets.Fonts.PixelArt;

        if(textPosition == Vector2.Zero)
        {
            showText = false;
        }
        this.textPosition = new Vector2(rectangle.X + textPosition.X, rectangle.Y + textPosition.Y);

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        drawBackground(spriteBatch);
        drawProgress(spriteBatch);
        drawSetoff(spriteBatch);
    }

    private void drawBackground(SpriteBatch spriteBatch)
    {
        if(background != null) spriteBatch.Draw(background, rectangle, Color.White);
    }

    public abstract void setValue(int percentage);
    protected abstract void drawProgress(SpriteBatch spriteBatch);

    public void setProgressColor(Color color)
    {
        this.progressColor = color;
    }

    public void setFontColor(Color color)
    {
        this.fontColor = color;
    }

    public void setDynamicColor(int percentage)
    {
        percentage = Math.Clamp(percentage, 0, 100);

        Color minColor = new Color(150, 0, 20);
        Color midColor = new Color(220, 130, 30);
        Color maxColor = new Color(0, 170, 50);

        if(percentage >= 50) this.progressColor = Color.Lerp(midColor, maxColor, (percentage - 50) / 50f );
        if(percentage < 50) this.progressColor = Color.Lerp(minColor, midColor, percentage / 50f);
    }

    protected void setTextPercentage(int percentage)
    {

        if(!showText) return;
        
        text = percentage.ToString(); // + "%";
        Vector2 textSize = font.MeasureString(text);
        textInnerPosition = new Vector2(textPosition.X - textSize.X / 2, textPosition.Y - textSize.Y / 2);
    }
    private void drawSetoff(SpriteBatch spriteBatch)
    {   

        if(showText) spriteBatch.DrawString(font, text, textInnerPosition, fontColor);
        if(setoff != null) spriteBatch.Draw(setoff, rectangle, Color.White);
        
    }


}