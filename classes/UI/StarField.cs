using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EXILION.UI;
public class Starfield
{
    private List<Star> stars;
    private Texture2D pixelTexture;
    private Random random;
    private int screenWidth;
    private int screenHeight;

    public Starfield(GraphicsDevice graphicsDevice, int totalStars)
    {
        screenWidth = graphicsDevice.Viewport.Width;
        screenHeight = graphicsDevice.Viewport.Height;
        random = new Random();
        stars = new List<Star>();

        pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });

        GenerateStars(totalStars);
    }

    private void GenerateStars(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Star star = new Star();
            
            star.Position = new Vector2(
                random.Next(0, screenWidth),
                random.Next(0, screenHeight)
            );

            // Determine layer
            int layer = random.Next(100);

            if (layer < 50)
            {
                star.Speed = 0.2f;
                star.Color = new Color(80, 80, 100) * 0.4f;
                star.Size = 1;
            }
            else if (layer < 80)
            {
                star.Speed = 0.6f;
                star.Color = new Color(180, 180, 210) * 0.7f;
                star.Size = 1;
            }
            else // Capa 3: CERCANAS
            {
                star.Speed = 1.2f;
                star.Color = Color.White;
                star.Size = 2;
            }

            stars.Add(star);
        }
    }

    public void Update(GameTime gameTime)
    {

        for (int i = 0; i < stars.Count; i++)
        {
            Star star = stars[i];

            star.Position.Y += star.Speed;

            if (star.Position.Y > screenHeight)
            {
                star.Position.Y = 0;
                star.Position.X = random.Next(0, screenWidth);
            }

            stars[i] = star;
        }

    }

    public void Draw(SpriteBatch spriteBatch, float alpha = 1.0f)
    {
        foreach (Star star in stars)
        {
            Rectangle destRect = new Rectangle(
                (int)star.Position.X, 
                (int)star.Position.Y, 
                star.Size, 
                star.Size
            );

            // Multiplicamos por alpha para permitir transiciones Fade-In/Out
            spriteBatch.Draw(pixelTexture, destRect, star.Color * alpha);
        }
    }
}