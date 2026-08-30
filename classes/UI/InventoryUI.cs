using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Entities.LivingThings;
using EXILION.Items;

namespace EXILION.UI;

public class InventoryUI
{
     private const int Columns = 8;
    private const int Rows = 3;     
    private const int SlotSize = 64;
    private const int SlotPadding = 4;

    private readonly Inventory inventory;
    private readonly Texture2D slotTexture;
    private readonly SpriteFont font;
    private readonly GameContext gameContext;

     private bool expanded = false;

      public InventoryUI(Inventory inventory, Texture2D slotTexture, SpriteFont font, GameContext gameContext)
    {
        this.inventory = inventory;
        this.slotTexture = slotTexture;
        this.font = font;
        this.gameContext = gameContext;
    }

     public void Update(InputManager input)
    {
        if (input.IsKeyPressed(Keys.I))
        {
            expanded = !expanded;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int slotSize = gameContext.ScaleX(SlotSize);
        int padding = gameContext.ScaleX(SlotPadding);

        int totalWidth = Columns * (slotSize + padding) - padding;
        int viewportWidth = gameContext.Game.GraphicsDevice.Viewport.Width;
        int viewportHeight = gameContext.Game.GraphicsDevice.Viewport.Height;

        int startX = (viewportWidth - totalWidth) / 2;
        int hotbarY = viewportHeight - slotSize - padding;

        DrawRow(spriteBatch, row: 0, x: startX, y: hotbarY, slotSize: slotSize, padding: padding);

        if (expanded)
        {
            for (int row = 1; row < Rows; row++)
            {
                int y = hotbarY - row * (slotSize + padding);
                DrawRow(spriteBatch, row, startX, y, slotSize, padding);
            }
        }
    }

     private void DrawRow(SpriteBatch spriteBatch, int row, int x, int y, int slotSize, int padding)
    {
        for (int col = 0; col < Columns; col++)
        {
            int slotIndex = row * Columns + col;
            int slotX = x + col * (slotSize + padding);
            var slotRect = new Rectangle(slotX, y, slotSize, slotSize);

            spriteBatch.Draw(slotTexture, slotRect, Color.White);

            ItemStack stack = inventory.GetSlot(slotIndex);
            if (stack == null) continue;

            spriteBatch.Draw(stack.Item.Icon, slotRect, Color.White);

            if (stack.Quantity > 1)
            {
                string quantityText = stack.Quantity.ToString();
                Vector2 textSize = font.MeasureString(quantityText);
                Vector2 textPosition = new Vector2(
                    slotRect.Right - textSize.X - 4,
                    slotRect.Bottom - textSize.Y - 2
                );

                spriteBatch.DrawString(font, quantityText, textPosition, Color.White);
            }
        }
    }


}