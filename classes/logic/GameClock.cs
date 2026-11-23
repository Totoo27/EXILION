namespace EXILION.Logic;

/// <summary>Authoritative day/night clock state without UI or audio dependencies.</summary>
public sealed class GameClock
{
    public const int DayLengthSeconds = 200;
    private const float TickIntervalSeconds = 1f;
    private float accumulator;

    public int RemainingSeconds { get; private set; } = DayLengthSeconds;
    public int DayCount { get; private set; } = 1;
    public bool IsNight { get; private set; }

    public void Restore(int remainingSeconds, int dayCount, bool isNight)
    {
        RemainingSeconds = remainingSeconds;
        DayCount = System.Math.Max(1, dayCount);
        IsNight = isNight;
        accumulator = 0f;
    }

    public bool Update(float deltaTime)
    {
        accumulator += deltaTime;
        if (accumulator < TickIntervalSeconds) return false;

        accumulator = 0f;
        RemainingSeconds--;
        if (RemainingSeconds > 0) return false;

        RemainingSeconds = DayLengthSeconds;
        if (IsNight) DayCount++;
        IsNight = !IsNight;
        return true;
    }
}
