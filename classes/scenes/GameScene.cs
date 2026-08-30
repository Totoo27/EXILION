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

    MapTile[,] map;
    MapRenderer mapRenderer;

    private const int MapWidth = 100;
    private const int MapHeight = 100;
    private const int TileSize = 32;
    private const int Seed = 12345; // más adelante: random o elegido por el jugador

    public GameScene(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        GameContext gameContext = Game.gameContext;

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        Texture2D texture = Assets.Sprites.Player;
        player = new Player(Vector2.Zero, new Sprite(texture, gameContext.ScaleXY(1)), gameContext);

        // --- Generación del mapa ---
        var generator = new MapGenerator(MapWidth, MapHeight, Seed);
        map = generator.Generate();

        Texture2D tileset = Assets.Sprites.Tileset; // agregá esta referencia en tu clase Assets
        mapRenderer = new MapRenderer(tileset, TileSize);
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (Game.input.IsKeyPressed(Keys.Escape))
        {
            Game.changeScene(new MainMenu(Game));
        }

        if(player != null)
        {
            player.Update(mouse.Position.ToVector2(), Game.input, gameTime);
            if (player.isDead)
            {
                player = null;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        mapRenderer.Draw(spriteBatch, map);

        if (player != null)
            player.Draw(spriteBatch, pixel);
    }
}