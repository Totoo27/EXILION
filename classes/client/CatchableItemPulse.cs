using System;
using Microsoft.Xna.Framework;

namespace EXILION.Client;

/// <summary>Presentation-only pulse for a dropped item.</summary>
public sealed class CatchableItemPulse
{
    private const float PulseSpeed = 1.5f;
    private const float PulseAmount = 0.12f;
    private const float SpawnDuration = 0.2f;
    private float _time;
    private float _age;

    public float Scale { get; private set; } = 0.55f;

    public void Update(float deltaTime)
    {
        _age += deltaTime;
        _time += deltaTime;
        float pulse = 1f + MathF.Sin(_time * PulseSpeed) * PulseAmount;
        if (_age < SpawnDuration)
        {
            float progress = Math.Clamp(_age / SpawnDuration, 0f, 1f);
            float eased = 1f - MathF.Pow(1f - progress, 3f);
            Scale = MathHelper.Lerp(0.55f, pulse, eased);
            return;
        }
        Scale = pulse;
    }
}
