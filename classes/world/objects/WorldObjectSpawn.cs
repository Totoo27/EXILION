namespace EXILION.World;

/// <summary>Describe dónde aparece un objeto antes de que el jugador pueda cambiarlo.</summary>
public readonly record struct WorldObjectSpawn(WorldObjectId Id, ObjectType Type, float TileX, float TileY);
