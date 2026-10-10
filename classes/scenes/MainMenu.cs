using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using EXILION.UI;

namespace EXILION.Scenes;

public class MainMenu : Scene, IHasSettings
{
    private SpriteFont font;
    private Button startGame;
    private Button settings;
    private Button quitGame;

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

    private GraphicsDevice GraphicsDevice;
    private InputManager input;
    private Camera camera;
    private IDisplaySettings displaySettings;
    

    public MainMenu(GameContext gameContext, GraphicsDevice graphicsDevice, InputManager input, IDisplaySettings displaySettings, Camera camera) : base(gameContext)
    {
        Music.Play(Assets.Songs.MenuMusic);
        this.GraphicsDevice = graphicsDevice;
        this.displaySettings = displaySettings;
        this.input = input;
        this.camera = camera;
    }

    public override void LoadContent()
    {

        settingsPanel = new SettingsPanel(GameContext, GraphicsDevice, input, this, displaySettings, new Vector2(GraphicsDevice.Viewport.Width, 0));
        settingsPanel.position.Y = getHalfScreenPositionY(settingsPanel.Height);

        // Stars
        starfield = new Starfield(GraphicsDevice, 200);

        // Sprites        
        title = Assets.Sprites.GameTitle;
        buttonSprite = Assets.Sprites.Button;
        sun = Assets.Sprites.Sun;

        // Font common buttons
        font = Assets.Fonts.PixelArt;

        // Buttons
        
        startGame = new Button("Start Game", new Rectangle((int)getHalfScreenPositionX(500), GameContext.ScaleY(400), GameContext.ScaleX(500), GameContext.ScaleY(80)), buttonSprite, Assets.Fonts.PixelArtBig);
        settings = new Button("Settings", new Rectangle((int)getHalfScreenPositionX(280), GameContext.ScaleY(520), GameContext.ScaleX(280), GameContext.ScaleY(50)), buttonSprite, font);
        quitGame = new Button("Quit Game", new Rectangle((int)getHalfScreenPositionX(280), GameContext.ScaleY(610), GameContext.ScaleX(280), GameContext.ScaleY(50)), buttonSprite, font);
        
        // Set original positions
        originalTitlePosition = new Vector2(getHalfScreenPositionX(900), GameContext.ScaleY(-30));
        originalSunPosition = new Vector2(getHalfScreenPositionX(900) + GameContext.ScaleX(410), GameContext.ScaleY(20));

        enabledSettingsPosition = new Vector2(getHalfScreenPositionX(settingsPanel.Width), getHalfScreenPositionY(settingsPanel.Height));

        // Rect initializations
        titleRect = new Rectangle((int)originalTitlePosition.X, (int)getHalfScreenPositionY(384), GameContext.ScaleX(900), GameContext.ScaleY(384));
        sunRect = new Rectangle((int)originalSunPosition.X, GameContext.ScaleY(-300), GameContext.ScaleX(300), GameContext.ScaleY(300));
    

    }
    public override void DrawUI(SpriteBatch spriteBatch)
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

        if(stopUpdating) return;

        MouseState mouseState = input.CurrentMouse;

        if (input.IsKeyPressed(Keys.Escape))
        {

            if (settingsPanel.enabled)
            {
                settingsDisableAnimation = true;
            } else
            {
                MainGame.Exit();
            }
            
        }

        starfield.Update(gameTime);
        updateTitleAnimation(gameTime);
        if(!animationFinished) return; // Prevent any logic before the animation ends


        settingsPanel.Update();
        updateSettingsAnimation();
        if(settingsPanel.enabled) return; // Prevent using Menu buttons while settings is enabled

        // Buttons update
        if (startGame.isClicked(mouseState))
        {
            SceneManager.ChangeScene(new GameScene(GameContext, input, camera, GraphicsDevice, displaySettings, "Saves/testSave.json"));
            return;
        }

        if (settings.isClicked(mouseState))
        {
            
            settingsPanel.enabled = true;

        }

        if (quitGame.isClicked(mouseState))
        {
            MainGame.Exit();
        }

    }

    public override void UnloadContent()
    {
        stopUpdating = true;

        // UI
        settingsPanel = null;
        startGame = null;
        settings = null;
        quitGame = null;
        starfield = null;
        title = null;
        sun = null;
        buttonSprite = null;
        font = null;

        // Other references
        GameContext = null;

        // Reset state
        titleRect = Rectangle.Empty;
        sunRect = Rectangle.Empty;

        originalSunPosition = Vector2.Zero;
        originalTitlePosition = Vector2.Zero;
        enabledSettingsPosition = Vector2.Zero;

        animationFinished = false;
        settingsDisableAnimation = false;
        timer = 0f;
        titleOpacity = 0f;
        buttonsOpacity = 0f;

        currentState = TitleAnimationState.WaitingStart;
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

                int destinationPos = titleRect.Y + GameContext.ScaleY(60);
                if(sunRect.Y < destinationPos) 
                    {
                        int velocity = GameContext.ScaleY(getEasingSpeed(35, sunRect.Y, destinationPos));

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
                        int velocity = GameContext.ScaleY(getEasingSpeed(20, sunRect.Y, (int)originalSunPosition.Y));

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

    public void closeSettings()
    {
        
       settingsDisableAnimation = true;

    }

    private void updateSettingsAnimation()
    {
        if(!settingsPanel.enabled) return;

        float positionX = settingsPanel.position.X;

        if (positionX > enabledSettingsPosition.X && !settingsDisableAnimation)
        {

            int speed = GameContext.ScaleX(getEasingSpeed(10, (int)positionX, (int)enabledSettingsPosition.X));

            // Settings move to center
            settingsPanel.position.X -= speed;

            // Everything else move apart
            titleRect.X -= speed;
            sunRect.X -= speed;
            startGame.position.X -= speed;
            settings.position.X -= speed;
            quitGame.position.X -= speed;

        }

        if(settingsDisableAnimation && positionX < GraphicsDevice.Viewport.Width)
        {
            int speed = GameContext.ScaleX(getEasingSpeed(10, (int)positionX, GraphicsDevice.Viewport.Width));

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

        float positionX = (GraphicsDevice.Viewport.Width - GameContext.ScaleX(size)) / 2;
        return positionX;
    }

    private float getHalfScreenPositionY(int size)
    {
        if(size <= 0)
        {
            throw new Exception("size cannot be negative");
        }

        float positionY = (GraphicsDevice.Viewport.Height - GameContext.ScaleY(size)) / 2;
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