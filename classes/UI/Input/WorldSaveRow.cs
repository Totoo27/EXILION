using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;

public class WorldSaveRow
{
    
    // Bounds
    public Rectangle bounds
    {
        get
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height);
        }

        private set{}
    }

    private Rectangle deleteBounds
    {
        get
        {
            return new Rectangle((int)position.X + bounds.Width - (int)deleteSize, (int)position.Y, (int)deleteSize, (int)deleteSize);
        }
    }
    public Vector2 position;
    
    

    // base Row
    private float height;
    private float width;
    private bool isHovered;
    private Color baseColor = new Color(55, 45, 65);
    private Color hoveredColor = new Color(35, 25, 45);
    private Color color;

    // delete button
    private float deleteSize;
    private Color baseDeleteColor = new Color(200, 55, 55);
    private Color hoveredDeleteColor = new Color(155, 35, 35);
    private Color deleteColor;
    private bool isDeleteHovered;


    // Row info
    private string worldName;
    private int worldSeed;

    private MouseState previousMouse;
    private SpriteFont font;
    private GameContext gameContext;

    public WorldSaveRow(float width, float height, GameContext gameContext, string name, int seed)
    {
        this.height = height;
        this.width = width;

        this.worldName = name;
        this.worldSeed = seed;

        this.gameContext = gameContext;

        this.font = Assets.Fonts.PixelArt;

        deleteSize = gameContext.ScaleXY(30);
    }

    public void UpdatePositionY(int positionY)
    {
        this.position.Y = positionY;
    }

    public int isClicked(MouseState currentMouse)
    {
        int clickType = 0;

        Point mousePosition = currentMouse.Position;

        isHovered = bounds.Contains(mousePosition);
        isDeleteHovered = deleteBounds.Contains(mousePosition);

        deleteColor = isDeleteHovered ? hoveredDeleteColor : baseDeleteColor;
        color = isHovered ? hoveredColor : baseColor;

        bool leftClickReleased =
            currentMouse.LeftButton == ButtonState.Released &&
            previousMouse.LeftButton == ButtonState.Pressed;

        if (leftClickReleased)
        {
            if (isDeleteHovered)
            {
                clickType = 2;
            }
            else if (isHovered)
            {
                clickType = 1;
            }
        }

        previousMouse = currentMouse;

        return clickType;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, bounds, color);
        spriteBatch.Draw(pixel, deleteBounds, deleteColor);

        spriteBatch.DrawString(font, worldName, new Vector2(bounds.X + gameContext.ScaleX(15), bounds.Y + gameContext.ScaleY(10)), Color.White);
        

        spriteBatch.DrawString(font,$"Seed: {worldSeed}",new Vector2(bounds.X + gameContext.ScaleX(15), bounds.Y + gameContext.ScaleY(45)),Color.LightGray);
    }

}