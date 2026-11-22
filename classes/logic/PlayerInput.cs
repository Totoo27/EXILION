namespace EXILION.Logic;


public readonly record struct PlayerInput(
    bool MoveUp,
    bool MoveDown,
    bool MoveLeft,
    bool MoveRight,
    bool Run,
    bool PrimaryActionPressed,
    bool PrimaryActionCaptured);
