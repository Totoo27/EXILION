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

    private GameContext gameContext;
    private GraphicsDevice GraphicsDevice;
    private InputManager input;
    private Camera camera;
    private IDisplaySettings displaySettings;

    // Settings

    private SettingsPanel settingsPanel;

    public PausePanel(GameContext gameContext, GraphicsDevice GraphicsDevice, InputManager input, Camera camera, IDisplaySettings displaySettings)
    {
        this.gameContext = gameContext;
        this.GraphicsDevice = GraphicsDevice;
        this.input = input;
        this.camera = camera;
        this.displaySettings = displaySettings;

        this.Width = this.gameContext.ScaleX(300);
        this.Height = this.gameContext.ScaleY(400);

        position = new Vector2(
            (GraphicsDevice.Viewport.Width - Width) / 2f,
            (GraphicsDevice.Viewport.Height - Height) / 2f
        );

        LoadContent();
    }

    public void LoadContent()
    {

        borderSize = gameContext.ScaleX(4);

        settingsPanel = new SettingsPanel(gameContext, GraphicsDevice, input, this, displaySettings, position);
        settingsPanel.position = new Vector2(
            (GraphicsDevice.Viewport.Width - settingsPanel.Width) / 2f,
            (GraphicsDevice.Viewport.Height - settingsPanel.Height) / 2f
        );

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        panelTexture = new Texture2D(GraphicsDevice, 1, 100);

        initPanelTexture();

        borderTexture = new Texture2D(GraphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArt;
        fontBig = Assets.Fonts.PixelArtBig;
        int buttonWidth = gameContext.ScaleX(200);
        int buttonHeight = gameContext.ScaleY(50);

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
            bounds.Y - gameContext.ScaleY(35)
        );

    }

    public void Update()
    {
        if (!enabled) return;
        
        MouseState mouseState = input.CurrentMouse;

        settingsPanel.Update();
        if(settingsPanel.enabled) return;

        if (resumeButton.isClicked(mouseState))
        {
            this.enabled = false;
        }

        if (settingsButton.isClicked(mouseState))
        {
            settingsPanel.enabled = true;
        }

        if (quitButton.isClicked(mouseState))
        {
            SceneManager.ChangeScene(new MainMenu(gameContext, GraphicsDevice, input, displaySettings, camera));
            return;
        }

    }

    public void Draw(SpriteBatch spriteBatch)
    {

        if (!enabled) return;
        

        // Opaque background
        spriteBatch.Draw(
            pixel,
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
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