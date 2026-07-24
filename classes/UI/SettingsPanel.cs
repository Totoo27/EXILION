using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.UI;

public class SettingsPanel
{

    public Vector2 position;
    public int Width{ get; private set; }
    public int Height{ get; private set; }
    private Game game;
    public Rectangle bounds
    {
        get
        {
            return new Rectangle((int)position.X, (int)position.Y, Width, Height);
        }

        private set{}
    }

    private Texture2D pixelTexture;

    public bool enabled = false;

    public SettingsPanel(Game1 game, Vector2 position)
    {
        this.game = game;
        this.position = position;
        this.Width = game.gameContext.ScaleX(1000);
        this.Height = game.gameContext.ScaleY(600);

        this.LoadContent();
    }
    public void LoadContent()
    {
        pixelTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
    }
    public void Update()
    {
        if(!enabled) return;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if(!enabled) return;

        spriteBatch.Draw(pixelTexture, bounds, Color.White);
    }

}