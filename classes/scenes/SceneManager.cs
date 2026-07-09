using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Scenes;
public class SceneManager
{
    public Scene CurrentScene { get; private set; }

    public void ChangeScene(Scene scene)
    {
        CurrentScene?.UnloadContent();

        CurrentScene = scene;

        CurrentScene.LoadContent();
    }

    public void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        CurrentScene?.Draw(spriteBatch);
    }
}