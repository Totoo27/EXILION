using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using EXILION.UI;
using System.Runtime.CompilerServices;
using System.Dynamic;

namespace EXILION.Scenes;

public class MainMenu : Scene
{
    private SpriteFont font;
    private Button startGame;
    private Button settings;
    private Button quitGame;
    private GameContext gameContext;
    private KeyboardState previousKeyboardState;

    private Rectangle titleRect;
    private Rectangle sunRect;

    // Original positions
    private Vector2 originalSunPosition;
    private Vector2 originalTitlePosition;

    private Texture2D title;
    private Texture2D sun;
    private Texture2D buttonSprite;

    // Settings

    private SettingsPanel settingsPanel;
    private Vector2 enabledSettingsPosition;

    // ---- Animation
    private bool animationFinished = false;
    private bool settingsDisableAnimation = false;
    private float timer = 0;


    private float titleOpacity = 0;
    private float buttonsOpacity = 0;


    private enum TitleAnimationState
    {
        WaitingStart,
        MovingSun,
        MiddleWait,
        MovingTitle,
        Finished
    }

    private TitleAnimationState currentState = TitleAnimationState.WaitingStart;

    private Starfield starfield;
    

    public MainMenu(Game1 game) : base(game)
    {
        Music.Play(Assets.Songs.MenuMusic);
        this.gameContext = game.gameContext;
    }

    public override void LoadContent()
    {

        settingsPanel = new SettingsPanel(Game, new Vector2(Game.GraphicsDevice.Viewport.Width, 0));
        settingsPanel.position.Y = getHalfScreenPositionY(settingsPanel.Height);

        // Stars
        starfield = new Starfield(Game.GraphicsDevice, 200);

        // Sprites        
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
        originalTitlePosition = new Vector2(getHalfScreenPositionX(900), gameContext.ScaleY(-30));
        originalSunPosition = new Vector2(getHalfScreenPositionX(900) + gameContext.ScaleX(410), gameContext.ScaleY(20));

        enabledSettingsPosition = new Vector2(getHalfScreenPositionX(settingsPanel.Width), getHalfScreenPositionY(settingsPanel.Height));

        // Rect initializations
        titleRect = new Rectangle((int)originalTitlePosition.X, (int)getHalfScreenPositionY(384), gameContext.ScaleX(900), gameContext.ScaleY(384));
        sunRect = new Rectangle((int)originalSunPosition.X, gameContext.ScaleY(-300), gameContext.ScaleX(300), gameContext.ScaleY(300));
    

    }
    public override void Draw(SpriteBatch spriteBatch)
    {

        starfield.Draw(spriteBatch);
        spriteBatch.Draw(title, titleRect, Color.White * titleOpacity);
        spriteBatch.Draw(sun, sunRect, Color.White);

        startGame.Draw(spriteBatch, buttonsOpacity);
        settings.Draw(spriteBatch, buttonsOpacity);
        quitGame.Draw(spriteBatch, buttonsOpacity);

        settingsPanel.Draw(spriteBatch);

    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
        {

            if (settingsPanel.enabled)
            {
                settingsDisableAnimation = true;
            } else
            {
                Game.Exit();
            }
            
        }

        previousKeyboardState = keyboardState;

        starfield.Update(gameTime);
        updateTitleAnimation(gameTime);
        if(!animationFinished) return; // Prevent any logic before the animation ends


        settingsPanel.Update();
        updateSettingsAnimation();
        if(settingsPanel.enabled) return; // Prevent using Menu buttons while settings is enabled

        // Buttons update
        if (startGame.isClicked(Mouse.GetState()))
        {
            Game.changeScene(new GameScene(Game));
        }

        if (settings.isClicked(Mouse.GetState()))
        {
            
            settingsPanel.enabled = true;

        }

        if (quitGame.isClicked(Mouse.GetState()))
        {
            Game.Exit();
        }

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

                int destinationPos = titleRect.Y + gameContext.ScaleY(60);
                if(sunRect.Y < destinationPos) 
                    {
                        int velocity = gameContext.ScaleY(getEasingSpeed(35, sunRect.Y, destinationPos));

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
                        int velocity = gameContext.ScaleY(getEasingSpeed(20, sunRect.Y, (int)originalSunPosition.Y));

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

    private void updateSettingsAnimation()
    {
        if(!settingsPanel.enabled) return;

        float positionX = settingsPanel.position.X;

        if (positionX > enabledSettingsPosition.X && !settingsDisableAnimation)
        {

            int speed = gameContext.ScaleX(getEasingSpeed(10, (int)positionX, (int)enabledSettingsPosition.X));

            // Settings move to center
            settingsPanel.position.X -= speed;

            // Everything else move apart
            titleRect.X -= speed;
            sunRect.X -= speed;
            startGame.position.X -= speed;
            settings.position.X -= speed;
            quitGame.position.X -= speed;

        }

        if(settingsDisableAnimation && positionX < Game.GraphicsDevice.Viewport.Width)
        {
            int speed = gameContext.ScaleX(getEasingSpeed(10, (int)positionX, Game.GraphicsDevice.Viewport.Width));

            // Settings move to center
            settingsPanel.position.X += speed;

            // Everything else move apart
            titleRect.X += speed;
            sunRect.X += speed;
            startGame.position.X += speed;
            settings.position.X += speed;
            quitGame.position.X += speed;

        } else if(settingsDisableAnimation)
        {
            settingsDisableAnimation = false;
            settingsPanel.enabled = false;
        }

    }
    private float getHalfScreenPositionX(int size)
    {
        if(size <= 0)
        {
            throw new Exception("size cannot be negative");
        }

        float positionX = (Game.GraphicsDevice.Viewport.Width - gameContext.ScaleX(size)) / 2;
        return positionX;
    }

    private float getHalfScreenPositionY(int size)
    {
        if(size <= 0)
        {
            throw new Exception("size cannot be negative");
        }

        float positionY = (Game.GraphicsDevice.Viewport.Height - gameContext.ScaleY(size)) / 2;
        return positionY;
    }

    private int getEasingSpeed(int speedDelimiter, int ownPosition, int destinationPos)
    {

        if(speedDelimiter <= 0)
        {
            throw new Exception("speed Delimiter must be above 0");
        }

        return (int)Math.Ceiling(1 * Math.Abs(ownPosition - destinationPos) / (float)speedDelimiter);
    }

}