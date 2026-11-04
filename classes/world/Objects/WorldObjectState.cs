namespace EXILION.World;

public sealed record WorldObjectState(
    WorldObjectId Id,
    int? CurrentHealth,
    bool IsDestroyed);
