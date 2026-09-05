using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Entities.LivingThings;
using EXILION.UI.Bar;

namespace EXILION.UI.HUD;

public class HUD
{
    
    private RadialBar hungerBar;
    private RadialBar thirstBar;
    private RadialBar healthBar;
    private LinearBar oxygenBar;
    private LinearBar watchBar;
    private InventoryUI inventoryUI;
    private bool hideHUD = false;
    private Player player;
    private GameContext gameContext;
    private Game1 Game;
    private int DIURNAL_TIME;
    public HUD(Player player, Game1 game, int DIURNAL_TIME)
    {
        this.Game = game;
        this.gameContext = Game.gameContext;
        this.DIURNAL_TIME = DIURNAL_TIME;
        this.player = player;
        LoadContent();
    }

    public void LoadContent()
    {
        Rectangle hungerRectangle = new Rectangle(gameContext.ScaleX(970), gameContext.ScaleY(560), gameContext.ScaleX(64), gameContext.ScaleY(64));
        Rectangle thirstRectangle = new Rectangle(gameContext.ScaleX(970), gameContext.ScaleY(640), gameContext.ScaleX(64), gameContext.ScaleY(64));
        Rectangle healthRectangle = new Rectangle(gameContext.ScaleX(1040), gameContext.ScaleY(550), gameContext.ScaleX(150), gameContext.ScaleY(150));
        Rectangle watchRectangle = new Rectangle(gameContext.ScaleX(1200), gameContext.ScaleY(300), gameContext.ScaleX(60), gameContext.ScaleY(114));
        Rectangle oxygenRectangle = new Rectangle(gameContext.ScaleX(1200), gameContext.ScaleY(468), gameContext.ScaleX(65), gameContext.ScaleY(232));

        Rectangle oxygenProgressRectangle = new Rectangle(oxygenRectangle.X + gameContext.ScaleX(21), oxygenRectangle.Y + gameContext.ScaleY(70), gameContext.ScaleX(6), gameContext.ScaleY(150));
        Rectangle watchProgressRectangle = new Rectangle(watchRectangle.X, watchRectangle.Y + gameContext.ScaleY(44), watchRectangle.Width, gameContext.ScaleY(50));

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

        watchBar = new LinearBar(

            Assets.Sprites.watchMeter,
            Assets.Sprites.watchProgress,
            Assets.Sprites.watchSetOff,
            watchRectangle,
            watchProgressRectangle,
            true,
            DIURNAL_TIME

        );

        inventoryUI = new InventoryUI(
            player.Inventory,
            Assets.Sprites.Slot,
            Assets.Fonts.PixelArt,
            gameContext
        );

        hungerBar.setProgressColor(new Color(148, 55, 24));
        thirstBar.setProgressColor(new Color(79, 165, 184));
        healthBar.setProgressColor(new Color(0, 170, 50));
        oxygenBar.setProgressColor(new Color(0, 170, 50));
        watchBar.setProgressColor(Color.White);

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

        watchBar.setValue(0);
    }

    public void Update()
    {
        inventoryUI.Update(Game.input);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if(hideHUD) return;

        hungerBar.Draw(spriteBatch);
        thirstBar.Draw(spriteBatch);
        healthBar.Draw(spriteBatch);
        oxygenBar.Draw(spriteBatch);
        watchBar.Draw(spriteBatch);
        inventoryUI.Draw(spriteBatch);
    }

    public int getSelectedSlotIndex()
    {
        return inventoryUI.SelectedSlotIndex;
    }

    public void setTime(int value)
    {
        watchBar.setValue(value);
    }

    public void hide()
    {
        hideHUD = true;
    }

    public void toggle()
    {
        hideHUD = !hideHUD;
    }

}