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

    Texture2D backGround;
    public MainMenu(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        GameContext gameContext = Game.gameContext;
        
        //  backGround = Game.Content.Load<Texture2D>("background");

        startGame = new Button("Start Game", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(450), gameContext.ScaleX(300), gameContext.ScaleY(60)), Game.Content.Load<Texture2D>("button"));
        settings = new Button("Settings", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(520), gameContext.ScaleX(200), gameContext.ScaleY(50)), Game.Content.Load<Texture2D>("button"));
        quitGame = new Button("Quit Game", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(580), gameContext.ScaleX(200), gameContext.ScaleY(50)), Game.Content.Load<Texture2D>("button"));
        font = Game.Content.Load<SpriteFont>("Arial");
        
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        // Draw(backGround, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);

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
        }

        if (quitGame.isClicked(Mouse.GetState()))
        {
            Game.Exit();
        }

    }
}