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
    private Rectangle sunRect;

    private Texture2D backGround;
    private Texture2D title;
    private Texture2D sun;
    private Texture2D buttonSprite;
    
    public MainMenu(Game1 game) : base(game)
    {
        Music.Play(Assets.Songs.MenuMusic);
        this.gameContext = game.gameContext;
    }

    public override void LoadContent()
    {

        // Sprites        
        backGround = Assets.Sprites.MenuBackground;
        title = Assets.Sprites.GameTitle;
        buttonSprite = Assets.Sprites.Button;
        sun = Assets.Sprites.Sun;

        // Font common buttons
        font = Assets.Fonts.PixelArt;

        // Buttons
        
        startGame = new Button("Start Game", new Rectangle((int)getHalfScreenPositionX(500), gameContext.ScaleY(400), gameContext.ScaleX(500), gameContext.ScaleY(80)), buttonSprite, Assets.Fonts.PixelArtBig);
        settings = new Button("Settings", new Rectangle((int)getHalfScreenPositionX(280), gameContext.ScaleY(520), gameContext.ScaleX(280), gameContext.ScaleY(50)), buttonSprite, font);
        quitGame = new Button("Quit Game", new Rectangle((int)getHalfScreenPositionX(280), gameContext.ScaleY(610), gameContext.ScaleX(280), gameContext.ScaleY(50)), buttonSprite, font);
        
        // Rect initializations
        backgroundRect = new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        titleRect = new Rectangle((int)getHalfScreenPositionX(900) + gameContext.ScaleX(10), gameContext.ScaleY(-30), gameContext.ScaleX(900), gameContext.ScaleY(384));
        sunRect = new Rectangle((int)getHalfScreenPositionX(900) + gameContext.ScaleX(420), gameContext.ScaleY(20), gameContext.ScaleX(300), gameContext.ScaleY(300));
    }
    public override void Draw(SpriteBatch spriteBatch)
    {

        spriteBatch.Draw(backGround, backgroundRect, Color.White);
        spriteBatch.Draw(title, titleRect, Color.White);
        spriteBatch.Draw(sun, sunRect, Color.White);

        startGame.Draw(spriteBatch);
        settings.Draw(spriteBatch);
        quitGame.Draw(spriteBatch);
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

    private float getHalfScreenPositionX(int size)
    {
        float positionX = (Game.GraphicsDevice.Viewport.Width - gameContext.ScaleX(size)) / 2;
        return positionX;
    }
}