using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.UI;

public class CraftingUI
{
    private readonly Texture2D pixel;
    private readonly GameContext gameContext;

    private bool visible = false;

    public CraftingUI(GameContext gameContext)
    {
        this.gameContext = gameContext;

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

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!visible) return;

        int width = gameContext.ScaleX(400);
        int height = gameContext.ScaleY(300);

        int x = (gameContext.Game.GraphicsDevice.Viewport.Width - width) / 2;
        int y = (gameContext.Game.GraphicsDevice.Viewport.Height - height) / 2;

        Rectangle panel = new Rectangle(x, y, width, height);

        spriteBatch.Draw(pixel, panel, Color.DarkSlateGray);
    }
}