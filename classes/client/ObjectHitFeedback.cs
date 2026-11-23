using System.Collections.Generic;
using EXILION.World;

namespace EXILION.Client;

/// <summary>Client-only transient feedback for recently damaged world objects.</summary>
public sealed class ObjectHitFeedback
{
    private const float Duration = 0.25f;
    private readonly Dictionary<WorldObjectId, float> _remaining = new();

    public void Play(WorldObjectId id) => _remaining[id] = Duration;

    public void Update(float elapsedSeconds)
    {
        foreach (WorldObjectId id in new List<WorldObjectId>(_remaining.Keys))
        {
            float remaining = _remaining[id] - elapsedSeconds;
            if (remaining <= 0f) _remaining.Remove(id);
            else _remaining[id] = remaining;
        }
    }

    public float GetScale(WorldObjectId id)
    {
        if (!_remaining.TryGetValue(id, out float remaining)) return 1f;
        float progress = 1f - remaining / Duration;
        float eased = progress * progress * (3f - 2f * progress);
        return 0.85f + 0.15f * eased;
    }
}
