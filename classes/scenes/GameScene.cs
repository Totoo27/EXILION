using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.UI.Bar;
using System;

namespace EXILION.Scenes;
public class GameScene : Scene
{
    private Player player;

    private Texture2D pixel;
    private LinearBar hungerBar;
    

    public GameScene(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        GameContext gameContext = Game.gameContext;

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, gameContext.ScaleXY(1)), gameContext);

        Rectangle rectangle = new Rectangle(gameContext.ScaleX(0), gameContext.ScaleY(0), gameContext.ScaleX(100), gameContext.ScaleY(10));
        Rectangle progressRectangle = new Rectangle(gameContext.ScaleX(0), gameContext.ScaleY(0), gameContext.ScaleX(100), gameContext.ScaleY(10));

        hungerBar = new LinearBar(
            Assets.Sprites.testBG,
            Assets.Sprites.testProgress,
            rectangle,
            progressRectangle,
            false,
            player.hunger.max
            
        );

        player.HungerChanged += hungerBar.setValue;
    }
    public override void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();

        if (Game.input.IsKeyPressed(Keys.Escape))
        {
            Game.changeScene(new MainMenu(Game));
        }

        if(player != null)
        {
            player.Update(mouse.Position.ToVector2(), Game.input, gameTime);
            if (player.isDead)
            {
                player = null;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        player?.Draw(spriteBatch, pixel);
        hungerBar.Draw(spriteBatch);
    }
}