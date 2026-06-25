using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Sprite sprite;

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

        Texture2D texture = Content.Load<Texture2D>("jugador");
        sprite = new Sprite(texture, Vector2.Zero, 2f);

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            sprite.position.X -= sprite.speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            sprite.position.X += sprite.speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            sprite.position.Y += sprite.speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            sprite.position.Y -= sprite.speed;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.Draw(sprite.texture, sprite.rectangle, Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
