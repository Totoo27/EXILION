using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Entities.CatchableItems;
using EXILION.Items;
using EXILION.UI;
using EXILION.World;

namespace EXILION.Scenes;

public class GameScene : Scene
{
    Player player;
    Texture2D pixel;
    private InventoryUI inventoryUI;

    private List<CatchableItem> catchableItems;
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

        inventoryUI = new InventoryUI(
            player.Inventory,
            Assets.Sprites.Slot,
            Assets.Fonts.PixelArt,
            gameContext
        );

        catchableItems = new List<CatchableItem>
        {
            new CatchableItem(
                new ItemStack(ItemRegistry.Madera, 70),
                new Vector2(100, 100),
                new Sprite(ItemRegistry.Madera.Icon, gameContext.ScaleXY(1)),
                gameContext
            ),
            new CatchableItem(
                new ItemStack(ItemRegistry.Piedra, 3),
                new Vector2(200, 100),
                new Sprite(ItemRegistry.Piedra.Icon, gameContext.ScaleXY(1)),
                gameContext
            ),
        };
    }

    public override void Update(GameTime gameTime)
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

        if (player != null)
        {
            player.Update(mouse.Position.ToVector2(), Game.input, gameTime);
            inventoryUI.Update(Game.input);

            if (player.isDead)
            {
                player = null;
            }
        player.Update(mouseWorldPosition, Game.input, gameTime);
        if (player.isDead)
        {
            player = null;
        }

        if (player != null && Game.input.IsKeyPressed(Keys.E))
        {
            Rectangle playerHitbox = player.GetHitbox();

            foreach (var item in catchableItems)
            {
                if (item.Picked) continue;

                if (playerHitbox.Intersects(item.GetHitbox()))
                {
                    player.TryPickup(item);
                }
            }
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
        player.Draw(spriteBatch, pixel);

        foreach (var item in catchableItems)
        {
            item.Draw(spriteBatch, pixel);
        }

        inventoryUI.Draw(spriteBatch);
        mapRenderer.Draw(spriteBatch, world);

        if (player != null)
            player.Draw(spriteBatch, pixel);
    }
}