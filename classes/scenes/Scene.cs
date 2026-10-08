using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Scenes;

public abstract class Scene
{
    protected GameContext GameContext;
    protected bool stopUpdating = false;

    protected Scene(GameContext gameContext)
    {
        GameContext = gameContext;
    }
    public abstract void LoadContent();

    public virtual Matrix? CameraTransform => null;
    public abstract void Update(GameTime gameTime);

    public virtual void Draw(SpriteBatch spriteBatch) { }

    public virtual void DrawUI(SpriteBatch spriteBatch) { }

    public abstract void UnloadContent();
}