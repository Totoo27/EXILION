using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.UI.Bar;

namespace EXILION.Scenes;
public class GameScene : Scene
{
    private Player player;

    private Texture2D pixel;
    private Bar hungerBar;
    private Bar thirstBar;
    private Bar healthBar;
    private Bar oxygenBar;
    

    public GameScene(Game1 game) : base(game)
    {
    }

    public override void LoadContent()
    {
        GameContext gameContext = Game.gameContext;

        pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        player = new Player(Vector2.Zero, new Sprite(Assets.Sprites.Player, gameContext.ScaleXY(1)), gameContext);

        Rectangle hungerRectangle = new Rectangle(gameContext.ScaleX(850), gameContext.ScaleY(500), gameContext.ScaleX(96), gameContext.ScaleY(96));
        Rectangle thirstRectangle = new Rectangle(gameContext.ScaleX(850), gameContext.ScaleY(610), gameContext.ScaleX(96), gameContext.ScaleY(96));
        Rectangle healthRectangle = new Rectangle(gameContext.ScaleX(950), gameContext.ScaleY(500), gameContext.ScaleX(200), gameContext.ScaleY(200));
        Rectangle oxygenRectangle = new Rectangle(gameContext.ScaleX(1150), gameContext.ScaleY(468), gameContext.ScaleX(65), gameContext.ScaleY(232));
        
        Rectangle oxygenProgressRectangle = new Rectangle(oxygenRectangle.X + gameContext.ScaleX(21), oxygenRectangle.Y + gameContext.ScaleY(70), gameContext.ScaleX(6), gameContext.ScaleY(150));
        
        Vector2 meterTextPosition = new Vector2(hungerRectangle.Width/2, hungerRectangle.Height/2);

        hungerBar = new RadialBar(

            Assets.Sprites.meter,
            Assets.Sprites.meterProgress,
            Assets.Sprites.hungerIcon,
            hungerRectangle,
            player.hunger.max,
            Game.GraphicsDevice,
            meterTextPosition,
            35,
            325
            
        );

        thirstBar = new RadialBar(

            Assets.Sprites.meter,
            Assets.Sprites.meterProgress,
            Assets.Sprites.thirstIcon,
            thirstRectangle,
            player.thirst.max,
            Game.GraphicsDevice,
            meterTextPosition,
            35,
            325
            
        );

        healthBar = new RadialBar(

            Assets.Sprites.healthMeter,
            Assets.Sprites.healthProgress,
            healthRectangle,
            player.maxHealth,
            Game.GraphicsDevice,
            new Vector2(healthRectangle.Width/2, healthRectangle.Height/2),
            90,
            360
            
        );

        oxygenBar = new LinearBar(

            Assets.Sprites.oxygenMeter,
            Assets.Sprites.oxygenProgress,
            oxygenRectangle,
            oxygenProgressRectangle,
            true,
            player.oxygen.max,
            new Vector2(oxygenRectangle.Width/2, gameContext.ScaleY(52))
        );

        hungerBar.setProgressColor(new Color(148, 55, 24));
        thirstBar.setProgressColor(new Color(79, 165, 184));
        healthBar.setProgressColor(new Color(0, 170, 50));
        oxygenBar.setProgressColor(new Color(0, 170, 50));

        hungerBar.setFontColor(new Color(42, 168, 65));
        thirstBar.setFontColor(new Color(42, 168, 65));
        healthBar.setFontColor(new Color(42, 168, 65));
        oxygenBar.setFontColor(new Color(42, 168, 65));

        player.HungerChanged += hungerBar.setValue;

        player.ThirstChanged += thirstBar.setValue;

        player.HealthChanged += healthBar.setValue;
        player.HealthChanged += healthBar.setDynamicColor;

        player.OxygenChanged += oxygenBar.setValue;
        player.OxygenChanged += oxygenBar.setDynamicColor;
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
        healthBar.Draw(spriteBatch);
        oxygenBar.Draw(spriteBatch);
    }
}