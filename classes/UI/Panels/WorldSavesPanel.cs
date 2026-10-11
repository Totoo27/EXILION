using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.UI;

public class WorldSavesPanel
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

    private IHasWorldSaves scene;

    private Color borderColor = new Color(46, 15, 74);

    // Buttons
    private Button backButton;

    // Buttons config
    private Texture2D buttonSprite;
    private SpriteFont font;
    private Texture2D pixel;

    public bool enabled = false;

    private GameContext gameContext;
    private GraphicsDevice GraphicsDevice;
    private InputManager input;

    public WorldSavesPanel(GameContext gameContext, GraphicsDevice GraphicsDevice, InputManager input, IHasWorldSaves scene, Vector2 position)
    {

        this.gameContext = gameContext;
        this.GraphicsDevice = GraphicsDevice;
        this.input = input;
        this.scene = scene;

        this.position = position;

        this.Width = gameContext.ScaleX(1000);
        this.Height = gameContext.ScaleY(600);

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
    }

    public void Update()
    {
        if (!enabled) return;

        MouseState mouseState = input.CurrentMouse;

        updateButtonPosition(backButton, gameContext.ScaleX(40), Height - gameContext.ScaleY(90));

        if (backButton.isClicked(mouseState))
        {
            scene.closeWorldSaves();
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

    public void updateButtonPosition(Button button, int positionX, int positionY)
    {
        button.position.X = (int)position.X + positionX;
        button.position.Y = (int)position.Y + positionY;
    }

}