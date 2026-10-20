namespace EXILION.World;

/// <summary>Una dirección que permite reconocer el mismo objeto aunque se vuelva a cargar su zona.</summary>
public readonly record struct WorldObjectId(int Seed, int TileX, int TileY, int SpawnIndex);
