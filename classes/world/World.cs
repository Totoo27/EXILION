using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EXILION.World;

public class World
{
    private readonly MapGenerator _generator;
    private readonly Dictionary<(int, int), Chunk> _loadedChunks = new();

    public int TileSize { get; }
    public int RenderDistanceChunks { get; set; } = 2;

    public World(int seed, int tileSize)
    {
        _generator = new MapGenerator(seed);
        TileSize = tileSize;
    }

    public void UpdateAroundPosition(Vector2 worldPosition)
    {
        int playerChunkX = (int)MathF.Floor(worldPosition.X / (TileSize * Chunk.Size));
        int playerChunkY = (int)MathF.Floor(worldPosition.Y / (TileSize * Chunk.Size));

        var needed = new HashSet<(int, int)>();

        for (int dx = -RenderDistanceChunks; dx <= RenderDistanceChunks; dx++)
        {
            for (int dy = -RenderDistanceChunks; dy <= RenderDistanceChunks; dy++)
            {
                var key = (playerChunkX + dx, playerChunkY + dy);
                needed.Add(key);

                if (!_loadedChunks.ContainsKey(key))
                {
                    _loadedChunks[key] = _generator.GenerateChunk(key.Item1, key.Item2);
                }
            }
        }

        var toRemove = new List<(int, int)>();
        foreach (var key in _loadedChunks.Keys)
        {
            if (!needed.Contains(key)) toRemove.Add(key);
        }
        foreach (var key in toRemove) _loadedChunks.Remove(key);
    }

    public IEnumerable<Chunk> LoadedChunks => _loadedChunks.Values;
}