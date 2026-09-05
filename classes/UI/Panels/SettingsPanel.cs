using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;

public class SettingsPanel
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

    public SettingsPanel(Game1 game, IHasSettings scene, Vector2 position)
    {
        this.game = game;
        this.scene = scene;

        this.position = position;

        this.Width = game.gameContext.ScaleX(1000);
        this.Height = game.gameContext.ScaleY(600);

        LoadContent();
    }

    public void LoadContent()
    {

        this.instructionsPanel = new InstructionsPanel(game);
        GraphicsDevice graphicsDevice = game.GraphicsDevice;
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
                game.gameContext.ScaleX(180),
                game.gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        fullScreenButton = new Button(
            "FullScreen",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                game.gameContext.ScaleX(180),
                game.gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        instructionsButton = new Button(
            "How to play",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                game.gameContext.ScaleX(220),
                game.gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        SFXButton = new Button(
            "SFX",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                game.gameContext.ScaleX(100),
                game.gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        musicButton = new Button(
            "Music",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                game.gameContext.ScaleX(100),
                game.gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

    }

    public void Update()
    {
        if (!enabled) return;

        updateButtonPosition(SFXButton, game.gameContext.ScaleX(40), game.gameContext.ScaleY(20));
        updateButtonPosition(musicButton, game.gameContext.ScaleX(40), game.gameContext.ScaleY(80));
        updateButtonPosition(backButton, game.gameContext.ScaleX(40), Height - game.gameContext.ScaleY(90));
        updateButtonPosition(fullScreenButton, Width - game.gameContext.ScaleX(200), game.gameContext.ScaleY(20));
        updateButtonPosition(instructionsButton, Width - game.gameContext.ScaleX(240), Height - game.gameContext.ScaleY(90));

        instructionsPanel.Update();
        if(instructionsPanel.enabled) return;

        if (backButton.isClicked(Mouse.GetState()))
        {
            scene.closeSettings();
        }

        if (fullScreenButton.isClicked(Mouse.GetState()))
        {
            game.toggleFullScreen();
        }

        if(instructionsButton.isClicked(Mouse.GetState()))
        {
            instructionsPanel.enabled = true;
        }

        if (musicButton.isClicked(Mouse.GetState()))
        {
            Music.Toggle();
        }

        if (SFXButton.isClicked(Mouse.GetState()))
        {
            SFX.toggle();
        }

    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!enabled) return;

        int borderSize = game.gameContext.ScaleX(4);

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