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
    public MainMenu(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        startGame = new Button("Start Game", new Rectangle(25, 250, 300, 60), Game.Content.Load<Texture2D>("button"));
        settings = new Button("Settings", new Rectangle(25, 320, 200, 50), Game.Content.Load<Texture2D>("button"));
        quitGame = new Button("Quit Game", new Rectangle(25, 380, 200, 50), Game.Content.Load<Texture2D>("button"));
        font = Game.Content.Load<SpriteFont>("Arial");
        
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
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