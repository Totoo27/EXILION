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
    protected int value;
    protected int maxValue;
    protected Color progressColor = Color.White;
    protected Color fontColor = Color.White;
    protected Rectangle rectangle;
    
    private bool showText = true;
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

    public abstract void setValue(int value);
    protected abstract void drawProgress(SpriteBatch spriteBatch);

    public void setProgressColor(Color color)
    {
        this.progressColor = color;
    }

    public void setFontColor(Color color)
    {
        this.fontColor = color;
    }
    private void drawSetoff(SpriteBatch spriteBatch)
    {
        String text = value.ToString();

        Vector2 textSize = font.MeasureString(text);
        Vector2 position = new Vector2(textPosition.X - textSize.X / 2, textPosition.Y - textSize.Y / 2);

        spriteBatch.DrawString(font, text, position, fontColor);

        if(setoff != null) spriteBatch.Draw(setoff, rectangle, Color.White);
        
    }


}