namespace EXILION.World;

public class MapGenerator
{
    private readonly float _elevOffsetX, _elevOffsetY;
    private readonly float _moistOffsetX, _moistOffsetY;

    public MapGenerator(int seed)
    {
        var rng = new System.Random(seed);
        _elevOffsetX = (float)rng.NextDouble() * 10000f;
        _elevOffsetY = (float)rng.NextDouble() * 10000f;
        _moistOffsetX = (float)rng.NextDouble() * 10000f;
        _moistOffsetY = (float)rng.NextDouble() * 10000f;
    }

    public Chunk GenerateChunk(int chunkX, int chunkY)
    {
        var tiles = new MapTile[Chunk.Size, Chunk.Size];
        int baseX = chunkX * Chunk.Size;
        int baseY = chunkY * Chunk.Size;

        for (int lx = 0; lx < Chunk.Size; lx++)
        {
            for (int ly = 0; ly < Chunk.Size; ly++)
            {
                int worldX = baseX + lx;
                int worldY = baseY + ly;

                float elevation = SimplexNoise.Fractal(
                    worldX + _elevOffsetX, worldY + _elevOffsetY,
                    octaves: 5, persistence: 0.5f, scale: 0.03f);

                float moisture = SimplexNoise.Fractal(
                    worldX + _moistOffsetX, worldY + _moistOffsetY,
                    octaves: 4, persistence: 0.5f, scale: 0.05f);

                elevation = (elevation + 1f) / 2f;
                moisture = (moisture + 1f) / 2f;

                tiles[lx, ly] = new MapTile
                {
                    Elevation = elevation,
                    Moisture = moisture,
                    Type = ClassifyBiome(elevation, moisture)
                };
            }
        }

        return new Chunk(chunkX, chunkY, tiles);
    }

    private TileType ClassifyBiome(float elevation, float moisture)
    {
        if (elevation < 0.30f) return TileType.DeepWater;
        if (elevation < 0.38f) return TileType.Water;
        if (elevation < 0.42f) return TileType.Sand;
        if (elevation > 0.85f) return TileType.Snow;
        if (elevation > 0.70f) return TileType.Rock;

        if (moisture < 0.3f) return TileType.Sand;
        if (moisture < 0.6f) return TileType.Grass;
        return TileType.Forest;
    }
}