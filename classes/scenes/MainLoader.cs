using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Scenes;

public class MainLoader : Scene
{
    
    private const int totalTasks = 4;
    private static int completedTasks = 0;

    private float progress = 0f;
    private Vector2 textPosition;

    private SpriteFont font;
    private Rectangle backgroundRect;
    private Texture2D backGround;

    private bool contentLoaded = false;
    private Viewport viewPort;

    private ContentManager Content;
    private GraphicsDevice GraphicsDevice;

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

        if (!contentLoaded)
        {
            Assets.Load(Content);
            contentLoaded = true;
        }

        progress = (float)completedTasks/totalTasks;

        if(progress >= 1)
        {
            SceneManager.ChangeScene(new MainMenu(GameData));
            return;
        }
    }

    public override void DrawUI(SpriteBatch spriteBatch)
    {
        string text = (progress * 100).ToString() + "%";
        Vector2 textSize = font.MeasureString(text);
        textPosition = new Vector2(viewPort.Width/2 - textSize.X/2, viewPort.Height/2 - textSize.Y/2);
        spriteBatch.Draw(backGround, backgroundRect, Color.White);

        spriteBatch.DrawString(
            font,
            text,
            textPosition,
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

    public static async Task addCompletedTask()
    {
        await Task.Delay(5);
        completedTasks++;
    }

    public static void forceCompleteTask()
    {
        completedTasks++;
    }

}
