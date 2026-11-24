using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Scenes;

public class MainLoader : Scene
{
    
    private const int totalTasks = 20;
    private static int completedTasks = 0;

    private float progress = 0f;
    private Vector2 textPosition;
    private Vector2 loadingPosition;

    private SpriteFont font;
    private Rectangle backgroundRect;
    private Texture2D backGround;

    private bool contentLoaded = false;
    private Viewport viewPort;

    private ContentManager Content;
    private GraphicsDevice GraphicsDevice;
    private static String loadingText = "";
    private String loadingPoints = ".";
    private float loadingPointsTimer = 0f;
    private const float loadingPointsInterval = 0.25f;
    private Task loadTask;

    public MainLoader(IGameData gameData) : base(gameData)
    {
        this.GraphicsDevice = gameData.GetGraphicsDevice();
        viewPort = this.GraphicsDevice.Viewport;
        this.Content = gameData.GetContentManager();
    }

    public override void LoadContent()
    {

        font = Content.Load<SpriteFont>("Fonts/PixelArtBig");
        backgroundRect = new Rectangle(0, 0, viewPort.Width, viewPort.Height);
        backGround = Content.Load<Texture2D>("Sprites/UI/MainMenuBackground");
    }

    public override void Update(GameTime gameTime)
    {
        if(stopUpdating) return;

        loadingPointsTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (loadingPointsTimer >= loadingPointsInterval)
        {
            loadingPointsTimer %= loadingPointsInterval;
            ManageLoadingPoints();
        }

        if (!contentLoaded)
        {
            loadTask = Assets.Load(Content);
            contentLoaded = true;
        }

        if (loadTask != null && loadTask.IsFaulted) throw loadTask.Exception;

        progress = (loadTask != null && loadTask.IsCompletedSuccessfully) ? 1f : Math.Min(0.99f, (float)completedTasks / totalTasks);

        if(progress >= 1)
        {
            SceneManager.ChangeScene(new MainMenu(GameData));
            return;
        }
    }

    public override void DrawUI(SpriteBatch spriteBatch)
    {
        string text = ((int)(progress * 100)).ToString() + "%";
        string loading = "Loading " + loadingText + loadingPoints;
        Vector2 textSize = font.MeasureString(text);
        Vector2 loadingSize = font.MeasureString(loading);
        textPosition = new Vector2(viewPort.Width/2 - textSize.X/2, viewPort.Height/2 - textSize.Y/2);
        loadingPosition = new Vector2(0, viewPort.Height - loadingSize.Y);
        spriteBatch.Draw(backGround, backgroundRect, Color.White);

        spriteBatch.DrawString(
            font,
            text,
            textPosition,
            Color.White);

        spriteBatch.DrawString(
            font,
            loading,
            loadingPosition,
            Color.White);
    }

    public override void UnloadContent()
    {
        stopUpdating = true;

        font = null;
        backGround = null;

        backgroundRect = Rectangle.Empty;
        textPosition = Vector2.Zero;
        viewPort = default;

        progress = 0f;
        contentLoaded = false;
    }

    public static void addCompletedTask()
    {
        completedTasks++;
    }

    public static void forceCompleteTask()
    {
        completedTasks++;
    }

    public static void SetLoadingText(String text)
    {
        loadingText = text;
    }

    private void ManageLoadingPoints()
    {
        if(loadingPoints.Length >= 3) loadingPoints = ".";
        else loadingPoints += ".";
    }

}
