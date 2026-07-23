using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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

    public MainLoader(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        viewPort = Game.GraphicsDevice.Viewport;

        font = Game.Content.Load<SpriteFont>("Fonts/PixelArtBig");
        backgroundRect = new Rectangle(0, 0, viewPort.Width, viewPort.Height);
        backGround = Game.Content.Load<Texture2D>("Sprites/MainMenuBackground");
    }

    public override void Update(GameTime gameTime)
    {
        if (!contentLoaded)
        {
            Assets.Load(Game.Content);
            contentLoaded = true;
        }

        progress = (float)completedTasks/totalTasks;

        if(progress >= 1)
        {
            Game.changeScene(new MainMenu(Game));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
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

    public static async Task addCompletedTask()
    {
        //await Task.Delay(500);
        completedTasks++;
    }

}