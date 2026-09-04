using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EXILION;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; } = 1f;

    private float shakeTime;
    private float shakeIntensity;

    private Random random = new Random();

    private readonly Viewport _viewport;

    public Camera(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Shake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeTime = duration;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (shakeTime > 0)
        {
            shakeTime -= deltaTime;
        }
    }

    public void Follow(Vector2 targetWorldPosition)
    {
        Position = targetWorldPosition;
    }

    public Matrix GetViewMatrix()
    {
        Vector2 shakeOffset = Vector2.Zero;

        if (shakeTime > 0)
        {
            shakeOffset = new Vector2(
                (float)(random.NextDouble() * 2 - 1) * shakeIntensity,
                (float)(random.NextDouble() * 2 - 1) * shakeIntensity
            );
        }

        Vector2 finalPosition = Position + shakeOffset;

        return Matrix.CreateTranslation(-finalPosition.X, -finalPosition.Y, 0f) *
            Matrix.CreateScale(Zoom, Zoom, 1f) *
            Matrix.CreateTranslation(
                _viewport.Width / 2f,
                _viewport.Height / 2f,
                0f
            );
    }

    public void damageShake(int damage)
    {
        float intensity = MathHelper.Clamp(damage * 0.2f, 2f, 6f);

        Shake(intensity, 0.2f);
    }
}