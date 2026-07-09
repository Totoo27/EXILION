using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;

namespace EXILION.Scenes;
public class GameScene : Scene
{
    Player player;

    Texture2D pixel;

    public GameScene(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        Texture2D texture = Game.Content.Load<Texture2D>("jugador");
        player = new Player(Vector2.Zero, new Sprite(texture, 1f));
    }
    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        KeyboardState keyBoardState = Keyboard.GetState();

        if(player != null)
        {
            player.Update(mouse.Position.ToVector2(), keyBoardState, gameTime);
            if (player.isDead)
            {
                player = null;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        player.Draw(spriteBatch, pixel);
    }
}