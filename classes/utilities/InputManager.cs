using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace EXILION;
public class InputManager
{
    public KeyboardState CurrentKeyboard { get; private set; }
    public KeyboardState PreviousKeyboard { get; private set; }

    public MouseState CurrentMouse { get; private set; }
    public MouseState PreviousMouse { get; private set; }

    public void Update()
    {
        PreviousKeyboard = CurrentKeyboard;
        CurrentKeyboard = Keyboard.GetState();

        PreviousMouse = CurrentMouse;
        CurrentMouse = Mouse.GetState();
    }

    public bool IsKeyPressed(Keys key)
    {
        return CurrentKeyboard.IsKeyDown(key) && PreviousKeyboard.IsKeyUp(key);
    }

    public bool IsKeyHeld(Keys key)
    {
        return CurrentKeyboard.IsKeyDown(key);
    }

    public Vector2 MousePosition => CurrentMouse.Position.ToVector2();

     public bool IsLeftMousePressed()
    {
        return CurrentMouse.LeftButton == ButtonState.Pressed
            && PreviousMouse.LeftButton == ButtonState.Released;
    }

    
    public bool IsLeftMouseReleased()
    {
        return CurrentMouse.LeftButton == ButtonState.Released
            && PreviousMouse.LeftButton == ButtonState.Pressed;
    }
}