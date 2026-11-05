using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Entities.LivingThings;
using EXILION.Items;

namespace EXILION.UI;

public class CraftingUI
{
    #nullable enable

    private const int PanelWidth = 560;
    private const int PanelHeight = 300;
    private const int PanelMargin = 8;
    private const int ListWidth = 220;
    private const int RowHeight = 48;
    private const int RowSpacing = 4;
    private const int DetailLineHeight = 36;
    private const int IconPadding = 4;
    private const int TextPadding = 8;

    private const int CraftButtonHeight = 40;

    private readonly Texture2D pixel;
    private readonly SpriteFont font;
    private readonly GameContext gameContext;
    private readonly Player player;

    public bool visible { get; private set; } = false;
    public Recipe? SelectedRecipe { get; private set; }

    public CraftingUI(GameContext gameContext, SpriteFont font, Player player)
    {
        this.gameContext = gameContext;
        this.font = font;
        this.player = player;

        pixel = new Texture2D(
            gameContext.Game.GraphicsDevice,
            1,
            1
        );

        pixel.SetData(new[] { Color.White });
    }

    public void Toggle()
    {
        visible = !visible;
    }

    public void Update(InputManager input)
{
    if (!visible) return;

    if (input.IsLeftMousePressed())
    {
        for (int i = 0; i < RecipeRegistry.All.Count; i++)
        {
            if (GetRowRect(i).Contains(input.MousePosition))
            {
                SelectedRecipe = RecipeRegistry.All[i];
                break;
            }
        }

        if (SelectedRecipe != null && GetCraftButtonRect().Contains(input.MousePosition))
        {
            player.TryCraft(SelectedRecipe);
        }
    }
}

    private Rectangle GetPanelRect()
    {
        int width = gameContext.ScaleX(PanelWidth);
        int height = gameContext.ScaleY(PanelHeight);

        int x = (gameContext.Game.GraphicsDevice.Viewport.Width - width) / 2;
        int y = (gameContext.Game.GraphicsDevice.Viewport.Height - height) / 2;

        return new Rectangle(x, y, width, height);
    }

    private Rectangle GetRowRect(int index)
    {
        Rectangle panel = GetPanelRect();

        int margin = gameContext.ScaleX(PanelMargin);
        int rowHeight = gameContext.ScaleY(RowHeight);
        int spacing = gameContext.ScaleY(RowSpacing);

        return new Rectangle(
            panel.X + margin,
            panel.Y + margin + index * (rowHeight + spacing),
            gameContext.ScaleX(ListWidth),
            rowHeight
        );
    }

    private Rectangle GetDetailRect()
    {
        Rectangle panel = GetPanelRect();

        int margin = gameContext.ScaleX(PanelMargin);
        int x = panel.X + margin + gameContext.ScaleX(ListWidth) + margin;

        return new Rectangle(
            x,
            panel.Y + margin,
            panel.Right - margin - x,
            panel.Height - 2 * margin
        );
    }

    private Rectangle GetDetailLineRect(int index)
    {
        Rectangle detail = GetDetailRect();

        int lineHeight = gameContext.ScaleY(DetailLineHeight);
        int spacing = gameContext.ScaleY(RowSpacing);

        return new Rectangle(
            detail.X,
            detail.Y + index * (lineHeight + spacing),
            detail.Width,
            lineHeight
        );
    }

    private Rectangle GetCraftButtonRect()
    {
        Rectangle detail = GetDetailRect();
        int height = gameContext.ScaleY(CraftButtonHeight);

        return new Rectangle(detail.X, detail.Bottom - height, detail.Width, height);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!visible) return;

        spriteBatch.Draw(pixel, GetPanelRect(), new Color(0, 183, 239) * 0.7f);

        DrawRecipeList(spriteBatch);
        DrawDetail(spriteBatch);
    }

    private void DrawRecipeList(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < RecipeRegistry.All.Count; i++)
        {
            Recipe recipe = RecipeRegistry.All[i];
            Rectangle rowRect = GetRowRect(i);

            bool isSelected = recipe == SelectedRecipe;
            Color rowColor = isSelected ? Color.White * 0.8f : Color.LightSkyBlue * 0.5f;
            spriteBatch.Draw(pixel, rowRect, rowColor);

            DrawItemLine(spriteBatch, recipe.Result, recipe.Result.Name, rowRect, Color.White);
        }
    }

    private void DrawDetail(SpriteBatch spriteBatch)
    {
        if (SelectedRecipe == null)
        {
            Rectangle detail = GetDetailRect();
            spriteBatch.DrawString(font, "Elegi una receta", new Vector2(detail.X, detail.Y), Color.White);
            return;
        }

        Recipe recipe = SelectedRecipe;

        string title = recipe.Amount > 1
            ? recipe.Result.Name + " x" + recipe.Amount
            : recipe.Result.Name;
        DrawItemLine(spriteBatch, recipe.Result, title, GetDetailLineRect(0), Color.White);

        Rectangle labelLine = GetDetailLineRect(1);
        Vector2 labelSize = font.MeasureString("Ingredientes");
        spriteBatch.DrawString(
            font,
            "Ingredientes",
            new Vector2(labelLine.X, labelLine.Y + (labelLine.Height - labelSize.Y) / 2),
            Color.White
        );

        for (int i = 0; i < recipe.Ingredients.Count; i++)
        {
            Ingredient ingredient = recipe.Ingredients[i];
            Rectangle line = GetDetailLineRect(i + 2);

            int have = player.Inventory.GetItemCount(ingredient.Item);
            Color color = have >= ingredient.Quantity ? Color.LimeGreen : Color.OrangeRed;

            DrawItemLine(spriteBatch, ingredient.Item, ingredient.Item.Name, line, color);

            string count = have + "/" + ingredient.Quantity;
            Vector2 countSize = font.MeasureString(count);
            Vector2 countPosition = new Vector2(
                line.Right - countSize.X - gameContext.ScaleX(TextPadding),
                line.Y + (line.Height - countSize.Y) / 2
            );
            spriteBatch.DrawString(font, count, countPosition, color);
        }
        DrawCraftButton(spriteBatch, recipe);
    }

    private void DrawItemLine(SpriteBatch spriteBatch, Item item, string text, Rectangle line, Color textColor)
    {
        int iconPadding = gameContext.ScaleX(IconPadding);
        int iconSize = line.Height - 2 * iconPadding;
        Rectangle iconRect = new Rectangle(
            line.X + iconPadding,
            line.Y + iconPadding,
            iconSize,
            iconSize
        );

        if (item.Icon != null)
        {
            spriteBatch.Draw(item.Icon, iconRect, Color.White);
        }

        Vector2 textSize = font.MeasureString(text);
        Vector2 textPosition = new Vector2(
            iconRect.Right + gameContext.ScaleX(TextPadding),
            line.Y + (line.Height - textSize.Y) / 2
        );

        spriteBatch.DrawString(font, text, textPosition, textColor);
    }

    private void DrawCraftButton(SpriteBatch spriteBatch, Recipe recipe)
    {
        CraftResult status = player.EvaluateCraft(recipe);
        Rectangle button = GetCraftButtonRect();

        Color buttonColor = status == CraftResult.Success ? Color.LimeGreen : Color.Gray;
        spriteBatch.Draw(pixel, button, buttonColor);

        string label = "Craftear";
        Vector2 labelSize = font.MeasureString(label);
        spriteBatch.DrawString(
            font,
            label,
            new Vector2(
                button.X + (button.Width - labelSize.X) / 2,
                button.Y + (button.Height - labelSize.Y) / 2
            ),
            Color.White
        );

        if (status != CraftResult.Success)
        {
            string message = status == CraftResult.MissingIngredients
                ? "Faltan ingredientes"
                : "Inventario lleno";

            Vector2 messageSize = font.MeasureString(message);
            spriteBatch.DrawString(
                font,
                message,
                new Vector2(
                    button.X + (button.Width - messageSize.X) / 2,
                    button.Y - messageSize.Y - gameContext.ScaleY(4)
                ),
                Color.OrangeRed
            );
        }
    }
}