using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using EXILION.UI;
using System.Runtime.CompilerServices;

namespace EXILION.Scenes;

public class MainMenu : Scene
{
    private SpriteFont font;
    private Button startGame;
    private Button settings;
    private Button quitGame;
    private GameContext gameContext;

    private Rectangle titleRect;
    private Rectangle sunRect;

    private Vector2 originalSunPosition;
    private Vector2 originalTitlePosition;

    private Texture2D backGround;
    private Texture2D title;
    private Texture2D sun;
    private Texture2D buttonSprite;

    // Animation
    private bool animationFinished = false;
    private bool waiting = false;

    private Rectangle backgroundRect;
    private Rectangle backgroundCopyRect;
    private float timer = 0;

    // Opacity
    private float titleOpacity = 0;
    private float buttonsOpacity = 0;

    // Animation states
    private enum TitleAnimationState
    {
        WaitingStart,
        MovingSun,
        MiddleWait,
        MovingTitle,
        Finished
    }

    private TitleAnimationState currentState = TitleAnimationState.WaitingStart;
    

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
        
        // Set original positions
        originalTitlePosition = new Vector2((int)getHalfScreenPositionX(900), gameContext.ScaleY(-30));
        originalSunPosition = new Vector2((int)getHalfScreenPositionX(900) + gameContext.ScaleX(410), gameContext.ScaleY(20));

        // Rect initializations
        backgroundRect = new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        backgroundCopyRect = new Rectangle(0, -Game.GraphicsDevice.Viewport.Height, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        titleRect = new Rectangle((int)originalTitlePosition.X, (int)getHalfScreenPositionY(384), gameContext.ScaleX(900), gameContext.ScaleY(384));
        sunRect = new Rectangle((int)originalSunPosition.X, gameContext.ScaleY(-300), gameContext.ScaleX(300), gameContext.ScaleY(300));
    

    }
    public override void Draw(SpriteBatch spriteBatch)
    {

        // Draw Background
        spriteBatch.Draw(backGround, backgroundRect, Color.White);
        spriteBatch.Draw(backGround, backgroundCopyRect, Color.White);

        spriteBatch.Draw(title, titleRect, Color.White * titleOpacity);
        spriteBatch.Draw(sun, sunRect, Color.White);

        
        startGame.Draw(spriteBatch, buttonsOpacity);
        settings.Draw(spriteBatch, buttonsOpacity);
        quitGame.Draw(spriteBatch, buttonsOpacity);

        
    }

    public override void Update(GameTime gameTime)
    {

        updateBackgroundAnimation();

        updateTitleAnimation(gameTime);
        if(!animationFinished) return;

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

    private float getHalfScreenPositionY(int size)
    {
        float positionY = (Game.GraphicsDevice.Viewport.Height - gameContext.ScaleY(size)) / 2;
        return positionY;
    }

    private void updateTitleAnimation(GameTime gameTime)
    {
        if(animationFinished) return;

        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        float fadeSpeed = 0.03f;

        switch (currentState)
        {
            case TitleAnimationState.WaitingStart:
            
            titleOpacity += fadeSpeed * timer;

            if(titleOpacity >= 1f && timer >= 1.5f)
                {
                    timer = 0;
                    titleOpacity = 1f;
                    currentState = TitleAnimationState.MovingSun;
                }
            break;

            case TitleAnimationState.MovingSun:
            int destinationPos = titleRect.Y + gameContext.ScaleY(50);
            if(sunRect.Y < destinationPos) 
                {
                    int velocity = getEasingSpeed(20, sunRect.Y, destinationPos);

                    sunRect.Y += velocity;
                }
            else
                {  
                    timer = 0;
                    currentState = TitleAnimationState.MiddleWait;
                }
            break;

            case TitleAnimationState.MiddleWait:
            if(timer >= 0.5f)
                {
                    timer = 0;
                    currentState = TitleAnimationState.MovingTitle;
                }
            break;

            case TitleAnimationState.MovingTitle:
            if (titleRect.Y > originalTitlePosition.Y && sunRect.Y > originalSunPosition.Y)
                {
                    int velocity = getEasingSpeed(10, sunRect.Y, (int)originalSunPosition.Y);

                    sunRect.Y -= velocity;
                    titleRect.Y -= velocity;
                }
                else
                {
                    timer = 0;
                    currentState = TitleAnimationState.Finished;
                }
            break;

            case TitleAnimationState.Finished:

            if(buttonsOpacity < 1f)
                {
                    buttonsOpacity += fadeSpeed * timer;
                }
            else
                {
                    buttonsOpacity = 1f;    
                    animationFinished = true;
                }

            break;
        }

    }

    private void updateBackgroundAnimation()
    {

        backgroundRect.Y += 1;
        backgroundCopyRect.Y += 1;

        // Restart positions
        if(backgroundRect.Y > Game.GraphicsDevice.Viewport.Height)
        {
            backgroundRect.Y = -Game.GraphicsDevice.Viewport.Height + 1;
        }

        if(backgroundCopyRect.Y > Game.GraphicsDevice.Viewport.Height)
        {
            backgroundCopyRect.Y = -Game.GraphicsDevice.Viewport.Height + 1;
        }

    }

    private int getEasingSpeed(int speedDelimiter, int ownPosition, int destinationPos)
    {
        return (int)Math.Ceiling(1 * Math.Abs(ownPosition - (float)destinationPos) / speedDelimiter);
    }

}