using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using EXILION.UI;

namespace EXILION.Scenes;

public class MainMenu : Scene
{
    private SpriteFont font;
    private Button startGame;
    private Button settings;
    private Button quitGame;
    GameContext gameContext;

    Texture2D backGround;
    Texture2D title;
    public MainMenu(Game1 game) : base(game)
    {
        Music.Play(Assets.MenuMusic);
        this.gameContext = game.gameContext;
    }

    public override void LoadContent()
    {
        
        backGround = Game.Content.Load<Texture2D>("Background");
        title = Game.Content.Load<Texture2D>("title");

        startGame = new Button("Start Game", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(450), gameContext.ScaleX(300), gameContext.ScaleY(60)), Game.Content.Load<Texture2D>("button"));
        settings = new Button("Settings", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(520), gameContext.ScaleX(200), gameContext.ScaleY(50)), Game.Content.Load<Texture2D>("button"));
        quitGame = new Button("Quit Game", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(580), gameContext.ScaleX(200), gameContext.ScaleY(50)), Game.Content.Load<Texture2D>("button"));
        font = Game.Content.Load<SpriteFont>("Arial");
        
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        int panelWidth = Game.GraphicsDevice.Viewport.Width;
        int panelHeight = Game.GraphicsDevice.Viewport.Height;

        spriteBatch.Draw(backGround, new Rectangle(0, 0, panelWidth, panelHeight), Color.White);
        spriteBatch.Draw(title, new Rectangle((panelWidth - gameContext.ScaleX(900)) / 2, gameContext.ScaleY(-30), gameContext.ScaleX(900), gameContext.ScaleY(384)), Color.White);

        startGame.Draw(spriteBatch, font);
        settings.Draw(spriteBatch, font);
        quitGame.Draw(spriteBatch, font);
    }

    public override void Update(GameTime gameTime)
    {

        if (startGame.isClicked(Mouse.GetState()))
        {
            Game.changeScene(new GameScene(Game));
        }

        if (settings.isClicked(Mouse.GetState()))
        {
            // Game.changeScene(new SettingsMenu(Game));
            Console.WriteLine("Settings");
            Music.Stop();
        }

        if (quitGame.isClicked(Mouse.GetState()))
        {
            Game.Exit();
        }

    }
}