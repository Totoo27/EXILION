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

    protected Rectangle rectangle;
    
    public Bar(Texture2D background, Texture2D progress, Texture2D setoff, Rectangle rectangle, int maxValue)
    {

        this.background = background;
        this.setoff = setoff;
        this.progress = progress;
        this.rectangle = rectangle;
        this.maxValue = maxValue;

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

    protected abstract void drawProgress(SpriteBatch spriteBatch);

    private void drawSetoff(SpriteBatch spriteBatch)
    {
        if(setoff != null) spriteBatch.Draw(setoff, rectangle, Color.White);
        
    }


}