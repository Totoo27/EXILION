using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace EXILION.UI.Bar;

public class RadialBar : Bar
{
    
    private GraphicsDevice graphicsDevice;
    private Texture2D filledProgress;
    private float minAngle;
    private float maxAngle;

    public RadialBar(Texture2D background, Texture2D progress, Texture2D setoff, Rectangle rectangle, int maxValue, GraphicsDevice graphicsDevice, Vector2 textPosition, float minAngle = 0, float maxAngle = 360)
    : base(background, progress, setoff, rectangle, maxValue, textPosition)
    {
        this.minAngle = MathHelper.ToRadians(minAngle);
        this.maxAngle = MathHelper.ToRadians(maxAngle);
        this.graphicsDevice = graphicsDevice;
        filledProgress = progress;
        setValue(maxValue);
    }

    public RadialBar(Texture2D background, Texture2D progress, Rectangle rectangle, int maxValue, GraphicsDevice graphicsDevice, Vector2 textPosition, float minAngle = 0, float maxAngle = 360) : base(background, progress, null, rectangle, maxValue, textPosition)
    {
        this.minAngle = MathHelper.ToRadians(minAngle);
        this.maxAngle = MathHelper.ToRadians(maxAngle);
        this.graphicsDevice = graphicsDevice;
        filledProgress = progress;
        setValue(maxValue);
    }

    public RadialBar(Texture2D progress, Rectangle rectangle, int maxValue, GraphicsDevice graphicsDevice, Vector2 textPosition, float minAngle = 0, float maxAngle = 360) : base(null, progress, null, rectangle, maxValue, textPosition)
    {
        this.minAngle = MathHelper.ToRadians(minAngle);
        this.maxAngle = MathHelper.ToRadians(maxAngle);
        this.graphicsDevice = graphicsDevice;
        filledProgress = progress;
        setValue(maxValue);
    }


    private Texture2D CreateRadialMask(Texture2D sourceTexture, float percentage)
    {
        int width = sourceTexture.Width;
        int height = sourceTexture.Height;

        Color[] sourcePixels = new Color[width * height];
        sourceTexture.GetData(sourcePixels);

        Color[] pixels = new Color[width * height];

        float centerX = width / 2f;
        float centerY = height / 2f;
        float radius = Math.Min(width, height) / 2f;

        percentage = MathHelper.Clamp(percentage, 0f, 1f);

        float totalAngleRange = maxAngle - minAngle;
        float fillThreshold = minAngle + (totalAngleRange * percentage);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                float dx = x - centerX;
                float dy = y - centerY;

                float distance = MathF.Sqrt(dx * dx + dy * dy);

                if (distance > radius)
                {
                    pixels[index] = Color.Transparent;
                    continue;
                }
    
                float pixelAngle = MathF.Atan2(-dx, dy);

                if (pixelAngle < 0) pixelAngle += MathHelper.TwoPi;
    
                if (pixelAngle >= minAngle && pixelAngle <= fillThreshold)
                {
                    pixels[index] = sourcePixels[index];
                }
                else
                {
                    pixels[index] = Color.Transparent;
                }
            }
        }

        Texture2D texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(pixels);

        return texture;
    }

    public override void setValue(int value)
    {
        this.value = value;
        float percentage = value / (float)maxValue;
        if (filledProgress != progress) filledProgress.Dispose();
        filledProgress = CreateRadialMask(progress, percentage);

    }

    protected override void drawProgress(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(filledProgress, rectangle, progressColor);
    }

}