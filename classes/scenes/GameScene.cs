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
    private GameWindow gameWindow;

    // World Saving
    private WorldSaveData saveData;
    private WorldSaveService saveService = new WorldSaveService();
    private string path;
    private bool isNewGame;
    private bool isLoaded = false;
    private int Seed;

    public GameScene(GameContext gameContext, InputManager input, Camera camera, GraphicsDevice graphicsDevice, IDisplaySettings displaySettings, string path, GameWindow gameWindow) : base(gameContext)
    {
        this.input = input;
        this.camera = camera;
        this.graphicsDevice = graphicsDevice;
        this.displaySettings = displaySettings;
        this.path = path;
        this.gameWindow = gameWindow;

        saveData = saveService.LoadAsync(path).Result;
        isNewGame = saveData.IsNewWorld;

        Music.Play(songsQueue[currentSongIndex], 1f);
    }

    public override void LoadContent()
    {
        // Init Seed
        Seed = saveData.WorldSeed;

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        pausePanel = new PausePanel(GameContext, graphicsDevice, input, camera, displaySettings, gameWindow);
        deathPanel = new DeathPanel(GameContext, graphicsDevice, input, camera, displaySettings, gameWindow);

        deathPanel.Respawn += processPlayerSpawn;
        Music.musicStop += changeMusic;

        catchableItems = new List<CatchableItem>();

        if (isNewGame)
        {
            startNewGame();
            saveGame();
        }
        else
        {
            loadGame();
        }

        // Load world seed
        int tileSize = (int)GameContext.ScaleXY(64);
        world = new World.World(seed: Seed, tileSize: tileSize) { RenderDistanceChunks = 2 };

        player.position = world.FindSpawnPosition();   // NUEVO: arranca en un lugar caminable
        world.UpdateAroundPosition(player.position);

        // NUEVO: el renderer ahora recibe también la textura de objetos
        mapRenderer = new MapRenderer(Assets.Sprites.Tileset, Assets.Sprites.Objects, tileSize);

        isLoaded = true;
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

        if(!isLoaded) return;
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
        if (player != null) player.Draw(spriteBatch, pixel);

        mapRenderer.DrawObjects(spriteBatch, world, playerY, true);
    }

    private void processPlayerSpawn(bool isNewGame)
    {
        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, GameContext.ScaleXY(1)), GameContext); 
        if (!isNewGame)
        {
            player.Restore(saveData.Player);
        }

        HUD = new HUD(player, GameContext, graphicsDevice, input, DIURNAL_PRESET_TIME);

        // update stats from beggining
        HUD.setTime(DIURNAL_PRESET_TIME - diurnalCycleTime);
        player.ForceUpdateStats();

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