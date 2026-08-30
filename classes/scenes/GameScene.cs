using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Items;
using EXILION.UI;
using System.Collections.Generic;

namespace EXILION.Scenes;
public class GameScene : Scene
{
    Player player;

    Texture2D pixel;
    private InventoryUI inventoryUI;

    
    //Sistema momentaneo para prueba de Inventory
     private class GroundItem
    {
        public Item Item;
        public Vector2 Position;
        public int Quantity;
        public bool Picked;
    }

    private const float PickupRange = 50f;

    private List<GroundItem> groundItems;

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

       inventoryUI = new InventoryUI(
            player.Inventory,
            Assets.Sprites.Slot,
            Assets.Fonts.PixelArt,
            gameContext
        );

        //Borrar en verion futura
        groundItems = new List<GroundItem>
        {
            new GroundItem { Item = ItemRegistry.Madera, Position = new Vector2(100, 100), Quantity = 70 },
            new GroundItem { Item = ItemRegistry.Piedra, Position = new Vector2(200, 100), Quantity = 3 },
        };
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
            inventoryUI.Update(Game.input);
            if (player.isDead)
            {
                player = null;
            }

            if (Game.input.IsKeyPressed(Keys.E))
            {
                foreach (var groundItem in groundItems)
                {
                    if (groundItem.Picked) continue;

                    float distance = Vector2.Distance(player.position, groundItem.Position);
                    if (distance <= PickupRange)
                    {
                        int leftover = player.Inventory.AddItem(groundItem.Item, groundItem.Quantity);
                        groundItem.Picked = true;

                        System.Console.WriteLine(
                            $"Agarraste {groundItem.Quantity - leftover}x {groundItem.Item.Name}. " +
                            $"Total en inventario: {player.Inventory.GetItemCount(groundItem.Item)}");
                    }
                }
            }
        }

        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        player.Draw(spriteBatch, pixel);
         foreach (var groundItem in groundItems)
        {
            if (groundItem.Picked) continue;
            spriteBatch.Draw(groundItem.Item.Icon, groundItem.Position, Color.White);
        }

        inventoryUI.Draw(spriteBatch);
    }

    
}