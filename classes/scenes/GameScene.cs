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
        GameContext gameContext = Game.gameContext;

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        Texture2D texture = Assets.Sprites.Player;
        player = new Player(Vector2.Zero, new Sprite(texture, gameContext.ScaleXY(1)), gameContext);
    }
    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            Game.changeScene(new MainMenu(Game));
        }

        if(player != null)
        {
            player.Update(mouse.Position.ToVector2(), keyboardState, gameTime);
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