using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;

public class SettingsPanel
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

    private IHasSettings scene;

    private Color borderColor = new Color(46, 15, 74);

    // Buttons
    private Button backButton;
    private Button fullScreenButton;
    private Button musicButton;
    private Button SFXButton;
    private Button instructionsButton;

    // Buttons config
    private Texture2D buttonSprite;
    private SpriteFont font;
    private Texture2D pixel;

    // Instructions
    private InstructionsPanel instructionsPanel;

    public bool enabled = false;

    private GameContext gameContext;
    private GraphicsDevice GraphicsDevice;
    private InputManager input;
    private IDisplaySettings displaySettings;

    public SettingsPanel(GameContext gameContext, GraphicsDevice GraphicsDevice, InputManager input, IHasSettings scene, IDisplaySettings displaySettings, Vector2 position)
    {

        this.gameContext = gameContext;
        this.GraphicsDevice = GraphicsDevice;
        this.input = input;
        this.scene = scene;
        this.displaySettings = displaySettings;

        this.position = position;

        this.Width = gameContext.ScaleX(1000);
        this.Height = gameContext.ScaleY(600);

        LoadContent();
    }

    public void LoadContent()
    {

        this.instructionsPanel = new InstructionsPanel(gameContext, GraphicsDevice, input);
        GraphicsDevice graphicsDevice = GraphicsDevice;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        panelTexture = new Texture2D(graphicsDevice, 1, 100);

        initPanelTexture();

        borderTexture = new Texture2D(graphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArt;

        backButton = new Button(
            "Back",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(180),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        fullScreenButton = new Button(
            "FullScreen",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(180),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        instructionsButton = new Button(
            "How to play",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(220),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        SFXButton = new Button(
            "SFX",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(100),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        musicButton = new Button(
            "Music",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(100),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

    }

    public void Update()
    {
        if (!enabled) return;

        MouseState mouseState = input.CurrentMouse;

        updateButtonPosition(SFXButton, gameContext.ScaleX(40), gameContext.ScaleY(20));
        updateButtonPosition(musicButton, gameContext.ScaleX(40), gameContext.ScaleY(80));
        updateButtonPosition(backButton, gameContext.ScaleX(40), Height - gameContext.ScaleY(90));
        updateButtonPosition(fullScreenButton, Width - gameContext.ScaleX(200), gameContext.ScaleY(20));
        updateButtonPosition(instructionsButton, Width - gameContext.ScaleX(240), Height - gameContext.ScaleY(90));

        instructionsPanel.Update();
        if(instructionsPanel.enabled) return;

        if (backButton.isClicked(mouseState))
        {
            scene.closeSettings();
        }

        if (fullScreenButton.isClicked(mouseState))
        {
            displaySettings.ToggleFullScreen();
        }

        if(instructionsButton.isClicked(mouseState))
        {
            instructionsPanel.enabled = true;
        }

        if (musicButton.isClicked(mouseState))
        {
            Music.Toggle();
        }

        if (SFXButton.isClicked(mouseState))
        {
            SFX.toggle();
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


        SFXButton.Draw(spriteBatch, 1f);
        musicButton.Draw(spriteBatch, 1f);
        backButton.Draw(spriteBatch, 1f);
        fullScreenButton.Draw(spriteBatch, 1f);
        instructionsButton.Draw(spriteBatch, 1f);

        instructionsPanel.Draw(spriteBatch);
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

    public void updateButtonPosition(Button button, int positionX, int positionY)
    {
        button.position.X = (int)position.X + positionX;
        button.position.Y = (int)position.Y + positionY;
        // Cambiar los valores por positionX y positionY
    }

}