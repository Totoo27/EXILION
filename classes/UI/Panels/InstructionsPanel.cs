using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace EXILION.UI;

public class InstructionsPanel
{

    public Vector2 position;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Rectangle bounds
    {
        get
        {
            return new Rectangle(
                (int)position.X,
                (int)position.Y,
                Width,
                Height
            );
        }

        private set {}
    }

    private Texture2D panelTexture;
    private Texture2D borderTexture;

    private Color borderColor = new Color(46, 15, 74);

    // Buttons
    private Button backButton;

    // Buttons config
    private Texture2D buttonSprite;
    private SpriteFont font;

    private Texture2D pixel;

    // Instructions
    private SpriteFont instructionsFont;
    private string instructions =
            "HOW TO PLAY\n\n" +
            "WASD - Move\n" +
            "LShift - Sprint\n" +
            "Mouse - Aim\n" +
            "E - Grab Item\n" +
            "L - Consume Item\n" +
            "Numbers - select Hotbar\n" +
            "H - Show hitboxes\n" +
            "ESC - Pause";

    public bool enabled = false;

    private GameContext gameContext;
    private GraphicsDevice GraphicsDevice;
    private InputManager input;

    public InstructionsPanel(GameContext gameContext, GraphicsDevice graphicsDevice, InputManager input)
    {
        this.gameContext = gameContext;
        this.GraphicsDevice = graphicsDevice;
        this.input = input;

        this.Width = gameContext.ScaleX(800);
        this.Height = gameContext.ScaleY(500);

        position = new Vector2(
            (GraphicsDevice.Viewport.Width - Width) / 2f,
            (GraphicsDevice.Viewport.Height - Height) / 2f
        );

        LoadContent();
    }

    public void LoadContent()
    {
        GraphicsDevice graphicsDevice = GraphicsDevice;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        panelTexture = new Texture2D(graphicsDevice, 1, 100);

        initPanelTexture();

        borderTexture = new Texture2D(graphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArt;
        instructionsFont = Assets.Fonts.PixelArt;

        backButton = new Button(
            "Resume",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(180),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

    }

    public void Update()
    {

        if (!enabled) return;

        if (backButton.isClicked(input.CurrentMouse))
        {
            this.enabled = false;
        }

    }

    public void Draw(SpriteBatch spriteBatch)
    {

        if (!enabled) return;

        int borderSize = gameContext.ScaleX(4);

        Rectangle innerBounds = new Rectangle(
            bounds.X + borderSize,
            bounds.Y + borderSize,
            bounds.Width - borderSize * 2,
            bounds.Height - borderSize * 2
        );

        // Panel
        spriteBatch.Draw(
            pixel,
            innerBounds,
            Color.Black
        );

        spriteBatch.Draw(
            panelTexture,
            innerBounds,
            Color.White
        );

        // Borde superior
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X,
                bounds.Y,
                bounds.Width,
                borderSize
            ),
            borderColor
        );

        // Borde inferior
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X,
                bounds.Bottom - borderSize,
                bounds.Width,
                borderSize
            ),
            borderColor
        );

        // Borde izquierdo
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X,
                bounds.Y,
                borderSize,
                bounds.Height
            ),
            borderColor
        );

        // Borde derecho
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.Right - borderSize,
                bounds.Y,
                borderSize,
                bounds.Height
            ),
            borderColor
        );

        Vector2 textSize = instructionsFont.MeasureString(instructions);

        Vector2 textPosition = new Vector2(
            bounds.Center.X - textSize.X / 2f,
            bounds.Center.Y - textSize.Y / 2f
        );

        spriteBatch.DrawString(
            instructionsFont,
            instructions,
            textPosition,
            Color.White
        );

        backButton.Draw(spriteBatch, 1f);
    }

    public void initPanelTexture()
    {
        Color[] panelColors = new Color[100];

        for (int i = 0; i < 100; i++)
        {
            float progress = i / 99f;

            panelColors[i] = Color.Lerp(
                new Color(5, 5, 8),
                new Color(25, 15, 35),
                progress
            );

            panelColors[i] = new Color(panelColors[i], 0.5f);
        }

        panelTexture.SetData(panelColors);
    }

}