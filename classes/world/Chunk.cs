using System.Collections.Generic;

namespace EXILION.World;

public class Chunk
{
    public const int Size = 16;

    public int ChunkX { get; }
    public int ChunkY { get; }
    public MapTile[,] Tiles { get; }
    public List<WorldObject> Objects { get; }

    public Chunk(int chunkX, int chunkY, MapTile[,] tiles, List<WorldObject> objects)
    {
        ChunkX = chunkX;
        ChunkY = chunkY;
        Tiles = tiles;
        Objects = objects;
    }
}