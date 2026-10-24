namespace EXILION.World;

/// <summary>Guarda los cambios que necesitamos recordar cuando se vuelve a cargar una zona.</summary>
public sealed record WorldObjectState(
    WorldObjectId Id,
    int? CurrentHealth,
    bool IsDestroyed,
    bool IsCollected);
