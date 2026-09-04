using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Scenes;

public abstract class Scene
{
    protected Game1 Game;

    protected Scene(Game1 game)
    {
        Game = game;
    }
    public abstract void LoadContent();

    public virtual Matrix? CameraTransform => null;
    public abstract void Update(GameTime gameTime);

    public virtual void Draw(SpriteBatch spriteBatch) { }

    public virtual void DrawUI(SpriteBatch spriteBatch) { }

    public virtual void UnloadContent() { }
}