using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;

public interface IGameData
{
    GameContext GetGameContext();
    GameWindow GetGameWindow();
    InputManager GetInput();
    Camera GetCamera();
    GraphicsDevice GetGraphicsDevice();
    ContentManager GetContentManager();
    IDisplaySettings GetDisplaySettings();
}
