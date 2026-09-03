using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; } = 1f;

    private readonly Viewport _viewport;

    public Camera(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Follow(Vector2 targetWorldPosition)
    {
        Position = targetWorldPosition;
    }

    public Matrix GetViewMatrix()
    {
        return Matrix.CreateTranslation(-Position.X, -Position.Y, 0f) *
               Matrix.CreateScale(Zoom, Zoom, 1f) *
               Matrix.CreateTranslation(_viewport.Width / 2f, _viewport.Height / 2f, 0f);
    }
}