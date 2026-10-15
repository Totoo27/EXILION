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

        player.position = world.FindSpawnPosition();   // NUEVO: arranca en un lugar caminable
        world.UpdateAroundPosition(player.position);

        // NUEVO: el renderer ahora recibe también la textura de objetos
        mapRenderer = new MapRenderer(Assets.Sprites.Tileset, Assets.Sprites.Objects, tileSize);

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

            player.Update(mouseWorldPosition, Game.input, gameTime, world);   // NUEVO: pasa el mundo
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
        float playerY = player != null ? player.position.Y : float.MaxValue;   // NUEVO

        mapRenderer.Draw(spriteBatch, world);                                  // terreno
        mapRenderer.DrawObjects(spriteBatch, world, playerY, false);           // NUEVO: objetos detrás del jugador

        if (player != null)
            player.Draw(spriteBatch, pixel);

        mapRenderer.DrawObjects(spriteBatch, world, playerY, true);            // NUEVO: objetos delante del jugador
    }
}