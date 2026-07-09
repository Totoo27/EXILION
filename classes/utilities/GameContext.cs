using System;

namespace EXILION;

public sealed class GameContext
{

    public Game1 Game { get; private set; }
    public GameContext(Game1 game)
    {
        this.Game = game;
    }

    public bool showHitboxes = false;

    public int ScreenWidth { get; set; } = 1280;
    public int ScreenHeight { get; set; } = 720;

    public int ScaleX(int value)
    {
        return (int)(value * (Game.GraphicsDevice.Viewport.Width / (float)ScreenWidth));
    }

    public int ScaleY(int value)
    {
        return (int)(value * (Game.GraphicsDevice.Viewport.Height / (float)ScreenHeight));
    }

    public float ScaleXY(int value)
    {
        return value * Math.Min((float)Game.GraphicsDevice.Viewport.Width / ScreenWidth, (float)Game.GraphicsDevice.Viewport.Height / ScreenHeight);
    }

}