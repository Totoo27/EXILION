using System;

namespace EXILION.Client;

/// <summary>Presentation-only pulse for a dropped item.</summary>
public sealed class CatchableItemPulse
{
    private const float PulseSpeed = 1.5f;
    private const float PulseAmount = 0.12f;
    private float _time;

    public float Scale { get; private set; } = 1f;

    public void Update(float deltaTime)
    {
        _time += deltaTime;
        Scale = 1f + MathF.Sin(_time * PulseSpeed) * PulseAmount;
    }
}
