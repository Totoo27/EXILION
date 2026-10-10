using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Entities.CatchableItems;
using EXILION.Items;
using EXILION.UI;
using EXILION.World;
using EXILION.UI.HUD;
using EXILION.Saving;
using System;

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

    // Panels

    private PausePanel pausePanel;
    private DeathPanel deathPanel;


    // Player
    private Player player;
    private Texture2D pixel;
    private HUD HUD;
    private InputManager input;

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


    private Camera camera;
    private GraphicsDevice graphicsDevice;
    private IDisplaySettings displaySettings;

    // World Saving
    private WorldSaveData saveData;
    private WorldSaveService saveService = new WorldSaveService();
    private string path;
    private bool isNewGame;

    private int Seed = 12345;

    public GameScene(GameContext gameContext, InputManager input, Camera camera, GraphicsDevice graphicsDevice, IDisplaySettings displaySettings, string path) : base(gameContext)
    {
        this.input = input;
        this.camera = camera;
        this.graphicsDevice = graphicsDevice;
        this.displaySettings = displaySettings;
        this.path = path;
        isNewGame = !saveService.Exists(path);
        Console.WriteLine(isNewGame);
        if (isNewGame)
        {
            saveData = new WorldSaveData
            {
                WorldSeed = Seed,
                WorldName = "Test"
            };

            saveService.SaveAsync(path, saveData).Wait();
        } else
        {
            saveData = saveService.LoadAsync(path).Result;
        }


        Music.Play(songsQueue[currentSongIndex], 1f);
    }

    public override void LoadContent()
    {

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        pausePanel = new PausePanel(GameContext, graphicsDevice, input, camera, displaySettings);
        deathPanel = new DeathPanel(GameContext, graphicsDevice, input, camera, displaySettings);

        deathPanel.Respawn += processPlayerSpawn;
        Music.musicStop += changeMusic;

        catchableItems = new List<CatchableItem>();

        if (isNewGame)
        {
            startNewGame();
        }
        else
        {
            loadGame();
        }

        // Load world seed
        int tileSize = (int)GameContext.ScaleXY(64);
        world = new World.World(seed: Seed, tileSize: tileSize) { RenderDistanceChunks = 2 };
        world.UpdateAroundPosition(player.position);
        Texture2D tileset = Assets.Sprites.Tileset;
        mapRenderer = new MapRenderer(tileset, tileSize);
    }

    private void loadGame()
    {

        Seed = saveData.WorldSeed;
        processPlayerSpawn(false);
        
        foreach(var catchableItem in saveData.CatchableItems)
        {

            var item = ItemRegistry.GetById(catchableItem.ItemId);
            if (item == null) continue;

            catchableItems.Add(

                new CatchableItem(
                    new ItemStack(item, catchableItem.Quantity),
                    new Vector2(catchableItem.PositionX, catchableItem.PositionY),
                    new Sprite(item.Icon, GameContext.ScaleXY(1)),
                    GameContext
                )

            );
        }
    }

    public void saveGame()
    {
        saveData.Player = player.GetSaveData();
        saveData.CatchableItems.Clear();
        foreach (var catchableItem in catchableItems)
        {
            saveData.CatchableItems.Add(catchableItem.GetSaveData());
        }

        saveService.SaveAsync(path, saveData).Wait();
    }

    private void startNewGame()
    {
        processPlayerSpawn(true);
        
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.Madera, 64),
                new Vector2(100, 100),
                new Sprite(ItemRegistry.Madera.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.Piedra, 3),
                new Vector2(200, 100),
                new Sprite(ItemRegistry.Piedra.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.AguaPurificada, 2),
                new Vector2(300, 100),
                new Sprite(ItemRegistry.AguaPurificada.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.CarneCocinada, 2),
                new Vector2(400, 100),
                new Sprite(ItemRegistry.CarneCocinada.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.OxigenoEmbotellado, 2),
                new Vector2(500, 100),
                new Sprite(ItemRegistry.OxigenoEmbotellado.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
        catchableItems.Add(
            new CatchableItem(
                new ItemStack(ItemRegistry.HachaTest, 1),
                new Vector2(600, 100),
                new Sprite(ItemRegistry.HachaTest.Icon, GameContext.ScaleXY(1)),
                GameContext
            )
        );
    }

    public override void Update(GameTime gameTime)
    {
        if(stopUpdating) return;

        MouseState mouse = input.CurrentMouse;

        if(deathPanel.enabled)
        {
            deathPanel.Update();
            return;
        }

        if (input.IsKeyPressed(Keys.Escape))
        {
            pausePanel.enabled = true;
        }
        if (input.IsKeyPressed(Keys.F1) && player != null)
        {
            HUD.toggle();
        }
        if (input.IsKeyPressed(Keys.G))
        {
            saveGame();
        }

        if (pausePanel.enabled)
        {
            pausePanel.Update();
            return;
        } 
            
        
        if(player != null)
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
            player.Update(mouseWorldPosition, input, gameTime);

            if (input.IsKeyPressed(Keys.E))
            {
                Rectangle playerHitbox = player.GetHitbox();

                for (int i = catchableItems.Count - 1; i >= 0; i--)
                {
                    CatchableItem item = catchableItems[i];

                    if (playerHitbox.Intersects(item.GetHitbox()))
                    {
                        player.TryPickup(item);
                        if (item.Picked) catchableItems.RemoveAt(i);
                    }
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
        if (nightOpacity > 0) spriteBatch.Draw(pixel, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), nightColor * nightOpacity);

        // UI
        HUD?.Draw(spriteBatch);

        // Panels
        pausePanel.Draw(spriteBatch);
        deathPanel.Draw(spriteBatch);
    }

    public override void UnloadContent()
    {
        stopUpdating = true;

        // Events
        Music.musicStop -= changeMusic;
        deathPanel.Respawn -= processPlayerSpawn;
        if (player != null)
        {
            player.Inventory.UpdateInventoryUI -= HUD.inventoryUI.UpdateHandItem;
            player.HealthChanged -= camera.damageShake;
        } 

        // UI
        HUD = null;
        pausePanel = null;
        deathPanel = null;

        // Entities
        catchableItems?.Clear();
        catchableItems = null;

        // World
        world = null;
        mapRenderer = null;

        // Player
        player = null;

        // Utils
        pixel?.Dispose();
        pixel = null;
        GameContext = null;
        camera = null;
        input = null;

        // Music queue
        songsQueue.Clear();

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
        HUD.hide();

        player.HealthChanged -= camera.damageShake;
        player.Inventory.UpdateInventoryUI -= HUD.inventoryUI.UpdateHandItem;

        HUD = null;
        player = null;
        deathPanel.enabled = true;
    }

    private void processPlayerSpawn(bool isNewGame)
    {
        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, GameContext.ScaleXY(1)), GameContext); 
        if (!isNewGame)
        {
            player.Restore(saveData.Player);
        }

        Console.WriteLine(player);

        HUD = new HUD(player, GameContext, graphicsDevice, input, DIURNAL_PRESET_TIME);

        player.HealthChanged += camera.damageShake;
        player.Inventory.UpdateInventoryUI += HUD.inventoryUI.UpdateHandItem;
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