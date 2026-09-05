using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.UI.Bar;
using EXILION.Entities.CatchableItems;
using EXILION.Items;
using EXILION.UI;
using EXILION.World;
using EXILION.UI.HUD;

namespace EXILION.Scenes;

public class GameScene : Scene
{

    // Game stats

    private float timer = 0f;
    private const float TIMER_INTERVAL = 1f;
    private float nightOpacity = 0f;
    private const int DIURNAL_PRESET_TIME = 200; // in seconds
    private int diurnalCycleTime = DIURNAL_PRESET_TIME; // in seconds
    private const float MAX_NIGHT_OPACITY = 0.65f;
    private const float NIGHT_TRANSITION_SPEED = 0.15f;
    private bool night = false;
    private Color nightColor = new Color(15, 10, 35);

    // Settings

    private PausePanel pausePanel;


    // Player
    private Player player;
    private Texture2D pixel;
    private HUD HUD;

    // Songs queue
    private List<Song> songsQueue = new List<Song>
    {
        Assets.Songs.exiliated,
        Assets.Songs.drownInInterrogations
    };

    private int currentSongIndex = 0;

    private List<CatchableItem> catchableItems;
    private World.World world;
    private MapRenderer mapRenderer;
    private GameContext gameContext;
    private Camera camera;

    private const int Seed = 12345;

    public GameScene(Game1 game) : base(game)
    {
        Music.Play(songsQueue[currentSongIndex], 1f);
        gameContext = Game.gameContext;
        camera = Game.camera;
    }

    public override void LoadContent()
    {

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, gameContext.ScaleXY(1)), gameContext); 
        HUD = new HUD(player, Game, DIURNAL_PRESET_TIME);

        player.HealthChanged += camera.damageShake;
        Music.musicStop += changeMusic;

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
            new CatchableItem(
                new ItemStack(ItemRegistry.AguaPurificada, 1),
                new Vector2(300, 100),
                new Sprite(ItemRegistry.AguaPurificada.Icon, gameContext.ScaleXY(1)),
                gameContext
            ),
            new CatchableItem(
                new ItemStack(ItemRegistry.AguaPurificada, 1),
                new Vector2(400, 100),
                new Sprite(ItemRegistry.AguaPurificada.Icon, gameContext.ScaleXY(1)),
                gameContext
            ),
            new CatchableItem(
                new ItemStack(ItemRegistry.AguaPurificada, 1),
                new Vector2(500, 100),
                new Sprite(ItemRegistry.AguaPurificada.Icon, gameContext.ScaleXY(1)),
                gameContext
            ),
        };

        int tileSize = (int)gameContext.ScaleXY(64);

        world = new World.World(seed: Seed, tileSize: tileSize) { RenderDistanceChunks = 2 };
        world.UpdateAroundPosition(player.position);

        Texture2D tileset = Assets.Sprites.Tileset;
        mapRenderer = new MapRenderer(tileset, tileSize);

        pausePanel = new PausePanel(Game);
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (Game.input.IsKeyPressed(Keys.Escape))
        {
            pausePanel.enabled = true;
        }
        if (Game.input.IsKeyPressed(Keys.F1) && player != null)
        {
            HUD.toggle();
        }

        pausePanel.Update();
        if(pausePanel.enabled) return;

        if (player != null)
        {
            Vector2 mouseWorldPosition = mouse.Position.ToVector2();

            if (camera != null)
            {
                Matrix inverseCamera = Matrix.Invert(camera.GetViewMatrix());
                mouseWorldPosition = Vector2.Transform(mouseWorldPosition, inverseCamera);
            }

            UpdateDiurnalCycle(gameTime);
            UpdateNightTransition(gameTime);

            HUD.Update();
            player.Update(mouseWorldPosition, Game.input, gameTime);

            if (Game.input.IsKeyPressed(Keys.E))
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
            
            if (Game.input.IsKeyPressed(Keys.L))
            {
                ItemStack selectedStack = player.Inventory.GetSlot(HUD.getSelectedSlotIndex());
                if (selectedStack != null)
                {
                    player.TryConsume(selectedStack.Item);
                }
            }
        
            world.UpdateAroundPosition(player.position);
            camera.Follow(player.position);

            if (player.isDead)
            {
                processPlayerDeath();
            }

        }

    }

    public override void Draw(SpriteBatch spriteBatch)
    {

        // Floor
        mapRenderer.Draw(spriteBatch, world);

        // Entities
        foreach (var item in catchableItems)
        {
            item.Draw(spriteBatch, pixel);
        }
        
        // Player
        player?.Draw(spriteBatch, pixel);
        
    }

    public override void DrawUI(SpriteBatch spriteBatch)
    {
        // Night Filter
        if (nightOpacity > 0) spriteBatch.Draw(pixel, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), nightColor * nightOpacity);

        // UI
        HUD.Draw(spriteBatch);

        // Pause panel
        pausePanel.Draw(spriteBatch);
    }

    private void UpdateDiurnalCycle(GameTime gameTime)
    {
        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (timer >= TIMER_INTERVAL)
        {
            timer = 0f;
            diurnalCycleTime--;

            if (diurnalCycleTime <= 0)
            {

                diurnalCycleTime = DIURNAL_PRESET_TIME;
                night = !night;

            }

            HUD.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);

        }

    }

    private void processPlayerDeath()
    {
        player = null;
        HUD.hide();
    }

    private void UpdateNightTransition(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (night)
        {
            nightOpacity += NIGHT_TRANSITION_SPEED * deltaTime;
            if (nightOpacity > MAX_NIGHT_OPACITY) nightOpacity = MAX_NIGHT_OPACITY;
        }
        else
        {
            nightOpacity -= NIGHT_TRANSITION_SPEED * deltaTime;
            if (nightOpacity < 0) nightOpacity = 0;
        }
    }

    private void changeMusic()
    {
        currentSongIndex++;
        if (currentSongIndex >= songsQueue.Count) currentSongIndex = 0;
        Music.Play(songsQueue[currentSongIndex], 3f);
    }

}