using Microsoft.Xna.Framework.Input;

namespace EXILION;
public class InputManager
{
    public KeyboardState CurrentKeyboard { get; private set; }
    public KeyboardState PreviousKeyboard { get; private set; }

    public void Update()
    {
        PreviousKeyboard = CurrentKeyboard;
        CurrentKeyboard = Keyboard.GetState();
    }

    public bool IsKeyPressed(Keys key)
    {
        return CurrentKeyboard.IsKeyDown(key) && PreviousKeyboard.IsKeyUp(key);
    }

    public bool IsKeyHeld(Keys key)
    {
        return CurrentKeyboard.IsKeyDown(key);
    }
}