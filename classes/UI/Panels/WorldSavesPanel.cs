using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Saving;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EXILION.Scenes;

namespace EXILION.UI;

public class WorldSavesPanel
{
    #nullable enable
    public Vector2 position;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Rectangle bounds
    {
        get
        {
            return new Rectangle(
                (int)position.X,
                (int)position.Y,
                Width,
                Height
            );
        }

        private set {}
    }

    private Texture2D panelTexture;
    private Texture2D borderTexture;

    private readonly IHasWorldSaves scene;

    private Color borderColor = new Color(46, 15, 74);

    // Buttons

    private Button backButton;
    private Button newWorldButton;

    // Buttons config
    private Texture2D buttonSprite;
    private SpriteFont font;
    private Texture2D pixel;

    public bool enabled = false;

    private GameContext gameContext;
    private GraphicsDevice GraphicsDevice;
    private InputManager input;

    // World saving management

    private readonly WorldSaveService saveService = new();
    private List<WorldSaveData> worlds = new();
    private List<WorldSaveRow> worldSaveRows = new();
    private bool isLoadingWorlds = false;
    private bool isCreatingWorld = false;
    private Camera camera;
    private IDisplaySettings displaySettings;
    private GameWindow gameWindow;
    private readonly WorldCreationPanel creationPanel;

    public WorldSavesPanel(GameContext gameContext, GraphicsDevice GraphicsDevice, InputManager input, IHasWorldSaves scene, Vector2 position, Camera camera, IDisplaySettings displaySettings, GameWindow window)
    {
        this.gameContext = gameContext;
        this.GraphicsDevice = GraphicsDevice;
        this.input = input;
        this.scene = scene;
        this.position = position;
        this.camera = camera;
        this.displaySettings = displaySettings;
        this.gameWindow = window;

        Width = gameContext.ScaleX(1000);
        Height = gameContext.ScaleY(600);

        LoadContent();

        creationPanel = new WorldCreationPanel(gameContext, GraphicsDevice, gameWindow);
        creationPanel.CreateRequested += OnCreateRequested;
        creationPanel.Cancelled += OnCreationCancelled;
    }


    public void LoadContent()
    {

        _ = RefreshWorldsAsync();

        GraphicsDevice graphicsDevice = GraphicsDevice;
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        panelTexture = new Texture2D(graphicsDevice, 1, 100);

        initPanelTexture();

        borderTexture = new Texture2D(graphicsDevice, 1, 1);
        borderTexture.SetData(new[] { Color.White });

        buttonSprite = Assets.Sprites.Button;
        font = Assets.Fonts.PixelArt;

        backButton = new Button(
            "Back",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(180),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );

        newWorldButton = new Button(
            "Create new world",
            new Rectangle(
                (int)position.X,
                (int)position.Y,
                gameContext.ScaleX(220),
                gameContext.ScaleY(50)
            ),
            buttonSprite,
            font
        );
    }

    public void Update()
    {
        if (!enabled || isLoadingWorlds) return;

        MouseState mouseState = input.CurrentMouse;

        if (creationPanel.IsOpen)
        {
            creationPanel.Update(mouseState);
            return;
        }

        updateButtonPosition(backButton, gameContext.ScaleX(40), gameContext.ScaleY(40));
        updateButtonPosition(newWorldButton, Width - gameContext.ScaleX(260), gameContext.ScaleY(40));

        if (backButton.isClicked(mouseState))
        {
            scene.closeWorldSaves();
        }

        if (newWorldButton.isClicked(mouseState))
        {
            creationPanel.Open(bounds);
            return;
        }

        if (isCreatingWorld || isLoadingWorlds) return;

        const int worldDelete = 2;
        const int loadWorld = 1;

        for (int i = 0; i < worldSaveRows.Count; i++)
        {
            int startOffset = gameContext.ScaleY(120);
            int spacing = gameContext.ScaleY(10);
            int startY = (spacing + worldSaveRows[i].bounds.Height) * i + bounds.Y + startOffset;

            worldSaveRows[i].UpdatePositionY(startY);
            int value = worldSaveRows[i].isClicked(input.CurrentMouse);

            if (value == worldDelete)
            {
                saveService.DeleteWorld(worlds[i].WorldId);
                _ = RefreshWorldsAsync();
                break;
            }

            if (value == loadWorld)
            {
                LoadWorld(worlds[i]);
            }
        }
    }

    private async void LoadWorld(WorldSaveData world)
    {
        if (isCreatingWorld) return;
        try
        {
            string path = saveService.GetWorldPath(world.WorldId);
            SceneManager.ChangeScene(new GameScene(gameContext, input, camera, GraphicsDevice, displaySettings, path, gameWindow));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar el mundo: {ex.Message}");
        }
    }

    private void OnCreateRequested(string worldName, int seed)
    {
        _ = CreateAndStartWorldAsync(worldName, seed);
    }

    private void OnCreationCancelled()
    {
    }
    private async Task CreateAndStartWorldAsync(string worldName, int seed)
    {
        if (isCreatingWorld) return;
        isCreatingWorld = true;
        try
        {
            WorldSaveData world = await saveService.CreateWorldAsync(worldName, seed);
            string path = saveService.GetWorldPath(world.WorldId);
            SceneManager.ChangeScene(new GameScene(gameContext, input, camera, GraphicsDevice, displaySettings, path, gameWindow));
            creationPanel.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear el mundo: {ex.Message}");
            creationPanel.ReportError("Could not create the world. Try again.");
        }
        finally
        {
            isCreatingWorld = false;
        }
    }

    public async Task RefreshWorldsAsync()
    {
        if (isLoadingWorlds) return;

        isLoadingWorlds = true;
        worlds.Clear();
        worldSaveRows.Clear();

        try
        {
            worlds = await saveService.GetAllWorldsAsync();

            for(int i = 0; i<worlds.Count; i++)
            {
                worldSaveRows.Add(new WorldSaveRow(
                    bounds.Width - gameContext.ScaleX(80),
                    gameContext.ScaleY(100),
                    gameContext,
                    worlds[i].WorldName,
                    worlds[i].WorldSeed
                ));

                worldSaveRows[i].position = new Vector2(gameContext.ScaleX(40) + bounds.X, bounds.Y);
            }
        }
        finally
        {
            isLoadingWorlds = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!enabled || isLoadingWorlds) return;

        int borderSize = gameContext.ScaleX(4);

        Rectangle innerBounds = new Rectangle(
            bounds.X + borderSize,
            bounds.Y + borderSize,
            bounds.Width - borderSize * 2,
            bounds.Height - borderSize * 2
        );

        // Panel
        spriteBatch.Draw(
            pixel,
            innerBounds,
            Color.Black
        );

        spriteBatch.Draw(
            panelTexture,
            innerBounds,
            Color.White
        );

        // Borde superior
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X,
                bounds.Y,
                bounds.Width,
                borderSize
            ),
            borderColor
        );

        // Borde inferior
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X,
                bounds.Bottom - borderSize,
                bounds.Width,
                borderSize
            ),
            borderColor
        );

        // Borde izquierdo
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.X,
                bounds.Y,
                borderSize,
                bounds.Height
            ),
            borderColor
        );

        // Borde derecho
        spriteBatch.Draw(
            borderTexture,
            new Rectangle(
                bounds.Right - borderSize,
                bounds.Y,
                borderSize,
                bounds.Height
            ),
            borderColor
        );

        backButton.Draw(spriteBatch, 1f);
        newWorldButton.Draw(spriteBatch, 1f);

        for (int i = 0; i < worldSaveRows.Count; i++)
        {
            worldSaveRows[i].Draw(spriteBatch, pixel);
        }

        creationPanel.Draw(spriteBatch);

    }

    public void initPanelTexture()
    {
        Color[] panelColors = new Color[100];

        for (int i = 0; i < 100; i++)
        {
            float progress = i / 99f;

            panelColors[i] = Color.Lerp(
                new Color(5, 5, 8),
                new Color(25, 15, 35),
                progress
            );

            panelColors[i] = new Color(panelColors[i], 0.5f);
        }

        panelTexture.SetData(panelColors);
    }

    public void updateButtonPosition(Button button, int positionX, int positionY)
    {
        button.position.X = (int)position.X + positionX;
        button.position.Y = (int)position.Y + positionY;
    }

}