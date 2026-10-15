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

    // ¿Puede estar parado acá un jugador de este radio (en píxeles)?
    public bool IsWalkable(Vector2 worldPosition, float radius = 0f)
    {
        // las 4 esquinas del cuadrado del jugador tienen que estar sobre tiles caminables
        if (!IsTileWalkable(worldPosition.X - radius, worldPosition.Y - radius)) return false;
        if (!IsTileWalkable(worldPosition.X + radius, worldPosition.Y - radius)) return false;
        if (!IsTileWalkable(worldPosition.X - radius, worldPosition.Y + radius)) return false;
        if (!IsTileWalkable(worldPosition.X + radius, worldPosition.Y + radius)) return false;

        return true;
    }

    // ¿El tile que está bajo este punto (en píxeles) es caminable?
    private bool IsTileWalkable(float pixelX, float pixelY)
    {
        int tileX = (int)MathF.Floor(pixelX / TileSize);
        int tileY = (int)MathF.Floor(pixelY / TileSize);

        int chunkX = (int)MathF.Floor((float)tileX / Chunk.Size);
        int chunkY = (int)MathF.Floor((float)tileY / Chunk.Size);

        int localX = tileX - chunkX * Chunk.Size;
        int localY = tileY - chunkY * Chunk.Size;

        if (_loadedChunks.TryGetValue((chunkX, chunkY), out var chunk))
        {
            return TileProperties.IsWalkable(chunk.Tiles[localX, localY].Type);
        }

        return true; // si el chunk todavía no está cargado, dejamos pasar en vez de trabar
    }

    // Busca el tile caminable más cercano al origen, para que el jugador no arranque en el agua
    public Vector2 FindSpawnPosition()
    {
        UpdateAroundPosition(Vector2.Zero); // asegura que haya chunks cargados cerca del origen

        Vector2 best = Vector2.Zero;
        float bestDistance = float.MaxValue;

        foreach (var chunk in _loadedChunks.Values)
        {
            for (int lx = 0; lx < Chunk.Size; lx++)
            {
                for (int ly = 0; ly < Chunk.Size; ly++)
                {
                    if (!TileProperties.IsWalkable(chunk.Tiles[lx, ly].Type)) continue;

                    // centro de este tile, en píxeles
                    float px = (chunk.ChunkX * Chunk.Size + lx + 0.5f) * TileSize;
                    float py = (chunk.ChunkY * Chunk.Size + ly + 0.5f) * TileSize;

                    float distance = px * px + py * py;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = new Vector2(px, py);
                    }
                }
            }
        }

        return best;
    }

    public IEnumerable<Chunk> LoadedChunks => _loadedChunks.Values;
}