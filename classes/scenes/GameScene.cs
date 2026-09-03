using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.World;

namespace EXILION.Scenes;

public class GameScene : Scene
{
    Player player;
    Texture2D pixel;

    World.World world;
    MapRenderer mapRenderer;
    Camera camera;

    private const int Seed = 12345;

    public GameScene(Game1 game) : base(game)
    {
    }

    public override Matrix? CameraTransform => camera?.GetViewMatrix();

    public override void LoadContent()
    {
        GameContext gameContext = Game.gameContext;

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        Texture2D texture = Assets.Sprites.Player;
        player = new Player(Vector2.Zero, new Sprite(texture, gameContext.ScaleXY(1)), gameContext);

        int tileSize = (int)gameContext.ScaleXY(64);

        world = new World.World(seed: Seed, tileSize: tileSize) { RenderDistanceChunks = 2 };
        world.UpdateAroundPosition(player.position);

        Texture2D tileset = Assets.Sprites.Tileset;
        mapRenderer = new MapRenderer(tileset, tileSize);

        camera = new Camera(Game.GraphicsDevice.Viewport);
    }

public override void Update(GameTime gameTime)
{
    MouseState mouse = Mouse.GetState();

    if (Game.input.IsKeyPressed(Keys.Escape))
    {
        Game.changeScene(new MainMenu(Game));
    }

    if (player != null)
    {
        Vector2 mouseWorldPosition = mouse.Position.ToVector2();

        if (camera != null)
        {
            Matrix inverseCamera = Matrix.Invert(camera.GetViewMatrix());
            mouseWorldPosition = Vector2.Transform(mouseWorldPosition, inverseCamera);
        }

        player.Update(mouseWorldPosition, Game.input, gameTime);
        if (player.isDead)
        {
            player = null;
        }
    }

    if (player != null)
    {
        world.UpdateAroundPosition(player.position);
        camera.Follow(player.position);
    }
}

    public override void Draw(SpriteBatch spriteBatch)
    {
        mapRenderer.Draw(spriteBatch, world);

        if (player != null)
            player.Draw(spriteBatch, pixel);
    }
}