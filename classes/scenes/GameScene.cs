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
    private Bar hungerBar;
    private Bar thirstBar;
    

    public GameScene(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        GameContext gameContext = Game.gameContext;

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, gameContext.ScaleXY(1)), gameContext);

        Rectangle hungerRectangle = new Rectangle(gameContext.ScaleX(850), gameContext.ScaleY(500), gameContext.ScaleX(100), gameContext.ScaleY(100));
        Rectangle thirstRectangle = new Rectangle(gameContext.ScaleX(850), gameContext.ScaleY(610), gameContext.ScaleX(100), gameContext.ScaleY(100));
        Vector2 meterTextPosition = new Vector2(hungerRectangle.Width/2, hungerRectangle.Height/2);

        hungerBar = new RadialBar(

            Assets.Sprites.meter,
            Assets.Sprites.meterProgress,
            Assets.Sprites.hungerIcon,
            hungerRectangle,
            player.hunger.max,
            Game.GraphicsDevice,
            meterTextPosition,
            40,
            320
            
        );

        thirstBar = new RadialBar(

            Assets.Sprites.meter,
            Assets.Sprites.meterProgress,
            Assets.Sprites.thirstIcon,
            thirstRectangle,
            player.thirst.max,
            Game.GraphicsDevice,
            meterTextPosition,
            40,
            320
            
        );

        hungerBar.setProgressColor(new Color(148, 55, 24));
        thirstBar.setProgressColor(new Color(79, 165, 184));

        hungerBar.setFontColor(new Color(42, 168, 65));
        thirstBar.setFontColor(new Color(42, 168, 65));

        player.HungerChanged += hungerBar.setValue;
        player.ThirstChanged += thirstBar.setValue;
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
        thirstBar.Draw(spriteBatch);
    }
}