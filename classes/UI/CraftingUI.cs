using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;

namespace EXILION.UI;

public class CraftingUI
{
    private const int PanelMargin = 8;
    private const int RowHeight = 48;
    private const int RowSpacing = 4;
    private const int IconPadding = 4;
    private const int TextPadding = 8;

    private readonly Texture2D pixel;
    private readonly SpriteFont font;
    private readonly GameContext gameContext;

    public bool visible { get; private set; } = false;

    public CraftingUI(GameContext gameContext, SpriteFont font)
    {
        this.gameContext = gameContext;
        this.font = font;

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

    private Rectangle GetPanelRect()
    {
        int width = gameContext.ScaleX(400);
        int height = gameContext.ScaleY(300);

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
            panel.Width - 2 * margin,
            rowHeight
        );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!visible) return;

        spriteBatch.Draw(pixel, GetPanelRect(), new Color (0,183,239));

        for (int i = 0; i < RecipeRegistry.All.Count; i++)
        {
            Recipe recipe = RecipeRegistry.All[i];
            Rectangle rowRect = GetRowRect(i);

            spriteBatch.Draw(pixel, rowRect, Color.LightSkyBlue * 0.7f);

            int iconPadding = gameContext.ScaleX(IconPadding);
            int iconSize = rowRect.Height - 2 * iconPadding;
            Rectangle iconRect = new Rectangle(
                rowRect.X + iconPadding,
                rowRect.Y + iconPadding,
                iconSize,
                iconSize
            );

            if (recipe.Result.Icon != null)
            {
                spriteBatch.Draw(recipe.Result.Icon, iconRect, Color.White);
            }

            Vector2 textSize = font.MeasureString(recipe.Result.Name);
            Vector2 textPosition = new Vector2(
                iconRect.Right + gameContext.ScaleX(TextPadding),
                rowRect.Y + (rowRect.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, recipe.Result.Name, textPosition, Color.White);
        }
    }
}