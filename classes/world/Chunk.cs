namespace EXILION.World;

public class Chunk
{
    public const int Size = 16;

    public int ChunkX { get; }
    public int ChunkY { get; }
    public MapTile[,] Tiles { get; }

    public Chunk(int chunkX, int chunkY, MapTile[,] tiles)
    {
        ChunkX = chunkX;
        ChunkY = chunkY;
        Tiles = tiles;
    }
}