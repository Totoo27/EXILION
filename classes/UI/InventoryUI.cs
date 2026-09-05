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

    private static readonly Keys[] HotbarSelectKeys =
    {
        Keys.D1, Keys.D2, Keys.D3, Keys.D4,
        Keys.D5, Keys.D6, Keys.D7, Keys.D8
    };

    private readonly Inventory inventory;
    private readonly Texture2D slotTexture;
    private readonly SpriteFont font;
    private readonly GameContext gameContext;

    private bool expanded = false;
    private int draggedSlotIndex = -1;
    private Vector2 mousePosition;

    public int SelectedSlotIndex { get; private set; } = 0;

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

        for (int i = 0; i < HotbarSelectKeys.Length; i++)
        {
            if (input.IsKeyPressed(HotbarSelectKeys[i]))
            {
                SelectedSlotIndex = i;
            }
        }

        mousePosition = input.MousePosition;

        if (input.IsLeftMousePressed())
        {
            int slotIndex = GetSlotIndexAt(mousePosition);
            if (slotIndex != -1 && inventory.GetSlot(slotIndex) != null)
            {
                draggedSlotIndex = slotIndex;
            }
        }

        if (input.IsLeftMouseReleased() && draggedSlotIndex != -1)
        {
            int targetIndex = GetSlotIndexAt(mousePosition);
            if (targetIndex != -1)
            {
                inventory.MoveItem(draggedSlotIndex, targetIndex);
            }
            draggedSlotIndex = -1;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var layout = GetLayout();

        DrawRow(spriteBatch, row: 0, layout);

        if (expanded)
        {
            for (int row = 1; row < Rows; row++)
            {
                DrawRow(spriteBatch, row, layout);
            }
        }

        if (draggedSlotIndex != -1)
        {
            ItemStack draggedStack = inventory.GetSlot(draggedSlotIndex);
            if (draggedStack != null)
            {
                var dragRect = new Rectangle(
                    (int)mousePosition.X - layout.SlotSize / 2,
                    (int)mousePosition.Y - layout.SlotSize / 2,
                    layout.SlotSize,
                    layout.SlotSize
                );
                spriteBatch.Draw(draggedStack.Item.Icon, dragRect, Color.White);
            }
        }
    }

    private void DrawRow(SpriteBatch spriteBatch, int row, Layout layout)
    {
        for (int col = 0; col < Columns; col++)
        {
            int slotIndex = row * Columns + col;
            Rectangle slotRect = GetSlotRect(row, col, layout);

            bool isSelected = row == 0 && col == SelectedSlotIndex;
            Color slotColor = isSelected ? Color.Yellow : Color.White;

            spriteBatch.Draw(slotTexture, slotRect, slotColor);

            if (slotIndex == draggedSlotIndex) continue;

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

    private int GetSlotIndexAt(Vector2 point)
    {
        Layout layout = GetLayout();
        int maxRow = expanded ? Rows : 1;

        for (int row = 0; row < maxRow; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Rectangle slotRect = GetSlotRect(row, col, layout);
                if (slotRect.Contains(point))
                {
                    return row * Columns + col;
                }
            }
        }

        return -1;
    }

    private Rectangle GetSlotRect(int row, int col, Layout layout)
    {
        int slotX = layout.StartX + col * (layout.SlotSize + layout.Padding);
        int y = row == 0
            ? layout.HotbarY
            : layout.HotbarY - row * (layout.SlotSize + layout.Padding);

        return new Rectangle(slotX, y, layout.SlotSize, layout.SlotSize);
    }

    private readonly struct Layout
    {
        public readonly int SlotSize;
        public readonly int Padding;
        public readonly int StartX;
        public readonly int HotbarY;

        public Layout(int slotSize, int padding, int startX, int hotbarY)
        {
            SlotSize = slotSize;
            Padding = padding;
            StartX = startX;
            HotbarY = hotbarY;
        }
    }

    private Layout GetLayout()
    {
        int slotSize = gameContext.ScaleX(SlotSize);
        int padding = gameContext.ScaleX(SlotPadding);

        int totalWidth = Columns * (slotSize + padding) - padding;
        int viewportWidth = gameContext.Game.GraphicsDevice.Viewport.Width;
        int viewportHeight = gameContext.Game.GraphicsDevice.Viewport.Height;

        int startX = (viewportWidth - totalWidth) / 2;
        int hotbarY = viewportHeight - slotSize - padding;

        return new Layout(slotSize, padding, startX, hotbarY);
    }
}