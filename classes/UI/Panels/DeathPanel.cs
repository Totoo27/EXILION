using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using EXILION.Scenes;

namespace EXILION.UI;

public class DeathPanel
{
    #nullable enable
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
    private Rectangle innerBounds;
    private int borderSize;

    // Buttons
    private Button respawnButton;
    private Button quitButton;

    // Buttons & Text config
    private Texture2D buttonSprite;
    private SpriteFont font;
    private SpriteFont fontBig;
    private string pauseText = "You Died";
    private Vector2 textPosition;

    private Texture2D pixel;
    public bool enabled = false;

    private GameContext gameContext;
    private GraphicsDevice GraphicsDevice;
    private InputManager input;
    private Camera camera;
    private IDisplaySettings displaySettings;

    // Events

    public event Action<bool>? Respawn;

    public DeathPanel(GameContext gameContext, GraphicsDevice GraphicsDevice, InputManager input, Camera camera, IDisplaySettings displaySettings)
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

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArtBig;
        fontBig = Assets.Fonts.PixelArtBig;
        int buttonWidth = gameContext.ScaleX(300);
        int buttonHeight = gameContext.ScaleY(75);

        int spacing = (Height - buttonHeight * 2) / 2;

        int buttonX = (int)position.X + (Width - buttonWidth) / 2;

        respawnButton = new Button(
            "Respawn",
            new Rectangle(
                buttonX,
                (int)position.Y + spacing,
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
                (int)position.Y + spacing * 2 + buttonHeight,
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

        if (respawnButton.isClicked(mouseState))
        {
            Respawn?.Invoke(true);
            enabled = false;
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
            Color.Black * 0.65f
        );

        spriteBatch.DrawString(
            fontBig,
            pauseText,
            textPosition,
            Color.White
        );

        respawnButton.Draw(spriteBatch, 1f);
        quitButton.Draw(spriteBatch, 1f);
    }

}