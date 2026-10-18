using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;

public class WorldSaveRow
{
    
    public Rectangle bounds
    {
        get
        {
            return new Rectangle((int)position.X, (int)position.Y, (int)height, (int)width);
        }

        private set{}
    }

    private float height;
    private float width;
    private bool isHovered;
    public Vector2 position;

    private MouseState previousMouse;


    private Color baseColor = new Color(55, 45, 65);
    private Color hoveredColor = new Color(35, 25, 45);
    private Color color;

    private string worldName;
    private int worldSeed;

    private SpriteFont font;

    private GameContext gameContext;

    public WorldSaveRow(float height, float width, GameContext gameContext, string name, int seed)
    {
        this.height = height;
        this.width = width;

        this.worldName = name;
        this.worldSeed = seed;

        this.gameContext = gameContext;

        this.font = Assets.Fonts.PixelArt;
    }

    public void UpdatePositionY(int positionY)
    {
        this.position.Y = positionY;
    }

    public bool isClicked(MouseState currentMouse)
    {
        bool clicked = false;
        isHovered = bounds.Contains(new Point(currentMouse.Position.X, currentMouse.Position.Y));

        if (isHovered)
        {
            color = hoveredColor;
        } else
        {
            color = baseColor;
        }

        if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed && this.bounds.Contains(new Point(currentMouse.Position.X, currentMouse.Position.Y)))
        {
            clicked = true;
        }

        previousMouse = currentMouse;

        return clicked;

    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, bounds, color);

        spriteBatch.DrawString(font, worldName, new Vector2(bounds.X + gameContext.ScaleX(15), bounds.Y + gameContext.ScaleY(10)), Color.White);

        spriteBatch.DrawString(font,$"Seed: {worldSeed}",new Vector2(bounds.X + gameContext.ScaleX(15), bounds.Y + gameContext.ScaleY(45)),Color.LightGray);
    }

}