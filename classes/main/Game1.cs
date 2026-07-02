using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Entity jugador;

    Texture2D pixel;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        Texture2D texture = Content.Load<Texture2D>("jugador");
        jugador = new Entity(Vector2.Zero, new Sprite(texture, 1f));

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            jugador.position.X -= jugador.speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            jugador.position.X += jugador.speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            jugador.position.Y += jugador.speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            jugador.position.Y -= jugador.speed;
        }

        MouseState mouse = Mouse.GetState();

        jugador.Update(mouse.Position.ToVector2());

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        jugador.Draw(_spriteBatch, pixel);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
