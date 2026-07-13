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
    private GameContext gameContext;

    private Rectangle backgroundRect;
    private Rectangle titleRect;

    Texture2D backGround;
    Texture2D title;
    public MainMenu(Game1 game) : base(game)
    {
        Music.Play(Assets.Songs.MenuMusic);
        this.gameContext = game.gameContext;
    }

    public override void LoadContent()
    {
        
        backGround = Game.Content.Load<Texture2D>("Sprites/MainMenuBackground");
        title = Game.Content.Load<Texture2D>("Sprites/exilionTitle");
        font = Assets.Fonts.PixelArt;

        startGame = new Button("Start Game", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(450), gameContext.ScaleX(300), gameContext.ScaleY(60)), Game.Content.Load<Texture2D>("Sprites/button"));
        settings = new Button("Settings", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(520), gameContext.ScaleX(200), gameContext.ScaleY(50)), Game.Content.Load<Texture2D>("Sprites/button"));
        quitGame = new Button("Quit Game", new Rectangle(gameContext.ScaleX(25), gameContext.ScaleY(580), gameContext.ScaleX(200), gameContext.ScaleY(50)), Game.Content.Load<Texture2D>("Sprites/button"));
        
        backgroundRect = new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        titleRect = new Rectangle((Game.GraphicsDevice.Viewport.Width - gameContext.ScaleX(900)) / 2, gameContext.ScaleY(-30), gameContext.ScaleX(900), gameContext.ScaleY(384));
    }
    public override void Draw(SpriteBatch spriteBatch)
    {

        spriteBatch.Draw(backGround, backgroundRect, Color.White);
        spriteBatch.Draw(title, titleRect, Color.White);

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