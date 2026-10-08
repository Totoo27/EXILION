using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Scenes;
using System;

namespace EXILION;

public class MainGame : Game, IDisplaySettings
{
    private static MainGame _instance;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager sceneManager;
    public InputManager input {get; private set;}
    public GameContext gameContext { get; private set; }
    public Camera camera {private set; get;}

    public MainGame()
    {
        _instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        gameContext = new GameContext(this);

        _graphics.PreferredBackBufferWidth = gameContext.ScreenWidth;
        _graphics.PreferredBackBufferHeight =  gameContext.ScreenHeight;

        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        camera = new Camera(GraphicsDevice.Viewport);
        
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
        sceneManager = new SceneManager();
        input = new InputManager();

        SceneManager.ChangeScene(new MainLoader(gameContext, GraphicsDevice, input, Content, camera, this));

    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here
        input.Update();

        if (input.IsKeyPressed(Keys.F11))
        {
            ToggleFullScreen();
        }

        camera.Update(gameTime);
        sceneManager.Update(gameTime);
        Music.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Draw the scene with camera transform
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, transformMatrix: camera.GetViewMatrix());

        sceneManager.Draw(_spriteBatch);

        _spriteBatch.End();


        // Fix to camera
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

        sceneManager.DrawUI(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void ToggleFullScreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();

        camera.updateViewPort(GraphicsDevice.Viewport);
    }

    public static void Exit()
    {
        ((Game)_instance).Exit();
    }

}
