using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Scenes;
using System;

namespace EXILION;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager sceneManager;

    public GameContext gameContext { get; private set; }

    private KeyboardState previousKeyboardState;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        gameContext = new GameContext(this);

        _graphics.PreferredBackBufferWidth = gameContext.ScreenWidth;
        _graphics.PreferredBackBufferHeight =  gameContext.ScreenHeight;

        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
        sceneManager = new SceneManager();
        sceneManager.ChangeScene(new MainLoader(this));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here

        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.F11) && !previousKeyboardState.IsKeyDown(Keys.F11))
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        sceneManager.Update(gameTime);

        base.Update(gameTime);

        previousKeyboardState = keyboardState;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        sceneManager.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void changeScene(Scene scene)
    {
        sceneManager.ChangeScene(scene);
    }
}
