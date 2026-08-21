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
    private Texture2D shadowTexture;

    private IHasSettings scene;

    // Back button
    private Button backButton;
    private Texture2D buttonSprite;
    private SpriteFont font;

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
        GraphicsDevice graphicsDevice = game.GraphicsDevice;

        panelTexture = new Texture2D(graphicsDevice, 1, 100);

        Color[] panelColors = new Color[100];

        for (int i = 0; i < 100; i++)
        {
            float progress = i / 99f;

            panelColors[i] = Color.Lerp(
                new Color(5, 5, 8),
                new Color(25, 15, 35),
                progress
            );
        }

        panelTexture.SetData(panelColors);

        borderTexture = new Texture2D(graphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        shadowTexture = new Texture2D(graphicsDevice, 1, 1);
        shadowTexture.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArt;

        backButton = new Button(
            "Back",
            new Rectangle(
                (int)position.X + game.gameContext.ScaleX(40),
                (int)position.Y + Height - game.gameContext.ScaleY(90),
                game.gameContext.ScaleX(180),
                game.gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );
    }

    public void Update()
    {
        if (!enabled) return;

        backButton.position.X = (int)position.X + game.gameContext.ScaleX(40);
        backButton.position.Y = (int)position.Y + Height - game.gameContext.ScaleY(90);

        if (backButton.isClicked(Mouse.GetState()))
        {
            scene.closeSettings();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!enabled)
            return;

        int borderSize = game.gameContext.ScaleX(4);
        int shadowSize = game.gameContext.ScaleX(12);

        Rectangle shadowBounds = new Rectangle(
            bounds.X + shadowSize / 2,
            bounds.Y + shadowSize / 2,
            bounds.Width + shadowSize,
            bounds.Height + shadowSize
        );

        spriteBatch.Draw(
            shadowTexture,
            shadowBounds,
            new Color(0, 0, 0, 100)
        );

        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X - borderSize,
                bounds.Y - borderSize,
                bounds.Width + borderSize * 2,
                bounds.Height + borderSize * 2
            ),
            new Color(80, 75, 100)
        );

        spriteBatch.Draw(
            borderTexture,
            bounds,
            new Color(125, 90, 160)
        );

        Rectangle innerBounds = new Rectangle(
            bounds.X + borderSize,
            bounds.Y + borderSize,
            bounds.Width - borderSize * 2,
            bounds.Height - borderSize * 2
        );

        spriteBatch.Draw(
            panelTexture,
            innerBounds,
            Color.White
        );

        backButton.Draw(spriteBatch, 1f);
    }
}