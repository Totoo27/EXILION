using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.UI;

public class ScrollList
{
    private Rectangle area;
    private int itemHeight;
    private int spacing;
    private int itemCount;
    private int firstVisible;
    private int visibleCount = 1;

    public int FirstVisible => firstVisible;
    public int EndIndex => Math.Min(itemCount, firstVisible + visibleCount); 
    private int MaxFirst => Math.Max(0, itemCount - visibleCount);

    
    public void Configure(Rectangle area, int itemHeight, int spacing, int itemCount)
    {
        this.area = area;
        this.itemHeight = itemHeight;
        this.spacing = spacing;
        this.itemCount = itemCount;

        visibleCount = Math.Max(1, (area.Height + spacing) / (itemHeight + spacing));
        firstVisible = Math.Clamp(firstVisible, 0, MaxFirst);
    }

    
    public void HandleWheel(int scrollDelta)
    {
        if (scrollDelta == 0) return;
        firstVisible = Math.Clamp(firstVisible - Math.Sign(scrollDelta), 0, MaxFirst);
    }

    public int GetItemY(int index) => area.Y + (index - firstVisible) * (itemHeight + spacing);

    public void DrawScrollBar(SpriteBatch spriteBatch, Texture2D pixel, int barWidth, int gap, int minThumbHeight)
    {
        if (MaxFirst <= 0) return;

        int x = area.Right + gap;
        var track = new Rectangle(x, area.Y, barWidth, area.Height);

        int thumbHeight = Math.Max(minThumbHeight, track.Height * visibleCount / itemCount);
        int thumbY = track.Y + (track.Height - thumbHeight) * firstVisible / MaxFirst;
        var thumb = new Rectangle(x, thumbY, barWidth, thumbHeight);

        spriteBatch.Draw(pixel, track, Color.White * 0.25f);
        spriteBatch.Draw(pixel, thumb, Color.White);
    }
}