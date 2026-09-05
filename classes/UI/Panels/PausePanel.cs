using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using EXILION.Scenes;

namespace EXILION.UI;

public class PausePanel : IHasSettings
{

    public Vector2 position;
    public int Width { get; private set; }
    public int Height { get; private set; }

    private Game1 game;

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
    private Rectangle innerBounds;
    private int borderSize;

    // Buttons
    private Button resumeButton;
    private Button settingsButton;
    private Button quitButton;

    // Buttons & Text config
    private Texture2D buttonSprite;
    private SpriteFont font;
    private SpriteFont fontBig;
    private String pauseText = "PAUSE";
    private Vector2 textPosition;

    private Texture2D pixel;
    public bool enabled = false;

    // Settings

    private SettingsPanel settingsPanel;

    public PausePanel(Game1 game)
    {
        this.game = game;

        this.Width = game.gameContext.ScaleX(300);
        this.Height = game.gameContext.ScaleY(400);

        position = new Vector2(
            (game.GraphicsDevice.Viewport.Width - Width) / 2f,
            (game.GraphicsDevice.Viewport.Height - Height) / 2f
        );

        LoadContent();
    }

    public void LoadContent()
    {

        borderSize = game.gameContext.ScaleX(4);

        settingsPanel = new SettingsPanel(game, this, position);
        settingsPanel.position = new Vector2(
            (game.GraphicsDevice.Viewport.Width - settingsPanel.Width) / 2f,
            (game.GraphicsDevice.Viewport.Height - settingsPanel.Height) / 2f
        );

        GraphicsDevice graphicsDevice = game.GraphicsDevice;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        panelTexture = new Texture2D(graphicsDevice, 1, 100);

        initPanelTexture();

        borderTexture = new Texture2D(graphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArt;
        fontBig = Assets.Fonts.PixelArtBig;
        int buttonWidth = game.gameContext.ScaleX(200);
        int buttonHeight = game.gameContext.ScaleY(50);

        int spacing = (Height - buttonHeight * 3) / 4;

        int buttonX = (int)position.X + (Width - buttonWidth) / 2;

        resumeButton = new Button(
            "Resume",
            new Rectangle(
                buttonX,
                (int)position.Y + spacing,
                buttonWidth,
                buttonHeight
            ),
            buttonSprite,
            font
        );

        settingsButton = new Button(
            "Settings",
            new Rectangle(
                buttonX,
                (int)position.Y + spacing * 2 + buttonHeight,
                buttonWidth,
                buttonHeight
            ),
            buttonSprite,
            font
        );

        quitButton = new Button(
            "Quit",
            new Rectangle(
                buttonX,
                (int)position.Y + spacing * 3 + buttonHeight * 2,
                buttonWidth,
                buttonHeight
            ),
            buttonSprite,
            font
        );

        innerBounds = new Rectangle(
            bounds.X + borderSize,
            bounds.Y + borderSize,
            bounds.Width - borderSize * 2,
            bounds.Height - borderSize * 2
        );

        Vector2 textSize = fontBig.MeasureString(pauseText);
        
        textPosition = new Vector2(
            bounds.Center.X - textSize.X / 2f,
            bounds.Y - game.gameContext.ScaleY(35)
        );

    }

    public void Update()
    {

        if (!enabled) return;

        settingsPanel.Update();
        if(settingsPanel.enabled) return;

        if (resumeButton.isClicked(Mouse.GetState()))
        {
            this.enabled = false;
        }

        if (settingsButton.isClicked(Mouse.GetState()))
        {
            settingsPanel.enabled = true;
        }

        if (quitButton.isClicked(Mouse.GetState()))
        {
            game.changeScene(new MainMenu(game));
        }

    }

    public void Draw(SpriteBatch spriteBatch)
    {

        if (!enabled) return;
        

        // Opaque background
        spriteBatch.Draw(
            pixel,
            new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height),
            Color.Black * 0.5f
        );

        spriteBatch.DrawString(
            fontBig,
            pauseText,
            textPosition,
            Color.White
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

        resumeButton.Draw(spriteBatch, 1f);
        settingsButton.Draw(spriteBatch, 1f);
        quitButton.Draw(spriteBatch, 1f);

        settingsPanel.Draw(spriteBatch);
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

    public void closeSettings()
    {
        settingsPanel.enabled = false;
    }

}