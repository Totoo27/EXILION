using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION;
using EXILION.Scenes;
using System;

namespace EXILION.UI;

public sealed class VictoryPanel
{
    private readonly IGameData gameData;
    private readonly InputManager input;
    private readonly GraphicsDevice graphicsDevice;
    private readonly Texture2D buttonTexture;
    private readonly SpriteFont font;
    private readonly SpriteFont titleFont;
    private readonly Button continueButton;
    private readonly Button menuButton;
    private readonly Starfield starfield;
    private readonly string title = "You escaped!";
    private readonly Vector2 titlePosition;

    public bool Enabled { get; set; }
    public event Action? ContinuePlaying;

    public VictoryPanel(IGameData gameData)
    {
        this.gameData = gameData;
        input = gameData.GetInput();
        graphicsDevice = gameData.GetGraphicsDevice();
        buttonTexture = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArtBig;
        titleFont = Assets.Fonts.PixelArtBig;

        GameContext context = gameData.GetGameContext();
        starfield = new Starfield(graphicsDevice, 200);
        int buttonWidth = context.ScaleX(420);
        int buttonHeight = context.ScaleY(75);
        int spacing = context.ScaleY(24);
        int centerX = graphicsDevice.Viewport.Width / 2;
        int firstButtonY = graphicsDevice.Viewport.Height / 2;

        continueButton = new Button(
            "Continue Playing",
            new Rectangle(centerX - buttonWidth / 2, firstButtonY, buttonWidth, buttonHeight),
            buttonTexture,
            font);
        menuButton = new Button(
            "Main Menu",
            new Rectangle(centerX - buttonWidth / 2, firstButtonY + buttonHeight + spacing, buttonWidth, buttonHeight),
            buttonTexture,
            font);

        Vector2 titleSize = titleFont.MeasureString(title);
        titlePosition = new Vector2(centerX - titleSize.X / 2f, firstButtonY - context.ScaleY(110));
    }

    public void Update(GameTime gameTime)
    {
        if (!Enabled) return;

        starfield.Update(gameTime);

        MouseState mouse = input.CurrentMouse;
        if (continueButton.isClicked(mouse))
        {
            Enabled = false;
            ContinuePlaying?.Invoke();
            return;
        }

        if (menuButton.isClicked(mouse))
        {
            SceneManager.ChangeScene(new MainMenu(gameData));
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!Enabled) return;

        spriteBatch.Draw(pixel, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.Black);
        starfield.Draw(spriteBatch);
        spriteBatch.DrawString(titleFont, title, titlePosition, Color.White);
        continueButton.Draw(spriteBatch);
        menuButton.Draw(spriteBatch);
    }
}
