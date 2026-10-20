using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EXILION.World;

public class World
{
    private readonly MapGenerator _generator;
    private readonly WorldObjectGenerator _objectGenerator;
    private readonly Dictionary<(int, int), Chunk> _loadedChunks = new();

    public int TileSize { get; }
    public int RenderDistanceChunks { get; set; } = 2;
    public WorldObjectStateStore ObjectStates { get; } = new();

    public World(int seed, int tileSize)
    {
        _generator = new MapGenerator(seed);
        _objectGenerator = new WorldObjectGenerator(seed);
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
                    var chunk = _generator.GenerateChunk(key.Item1, key.Item2);
                    PopulateObjects(chunk);
                    _loadedChunks[key] = chunk;
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

    // Comprueba que el jugador quepa en esta posición sin pisar agua.
    public bool IsWalkable(Vector2 worldPosition, float radius = 0f)
    {
        // Revisamos las cuatro esquinas para que el jugador no quede medio metido en el agua.
        if (!IsTileWalkable(worldPosition.X - radius, worldPosition.Y - radius)) return false;
        if (!IsTileWalkable(worldPosition.X + radius, worldPosition.Y - radius)) return false;
        if (!IsTileWalkable(worldPosition.X - radius, worldPosition.Y + radius)) return false;
        if (!IsTileWalkable(worldPosition.X + radius, worldPosition.Y + radius)) return false;

        return true;
    }

    // Busca el tile que hay debajo de este punto y comprueba si se puede caminar por él.
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

        return true; // Si todavía no cargamos esta parte del mapa, dejamos pasar al jugador.
    }

    // Encuentra un lugar seco cerca del inicio para que el jugador no aparezca en el agua.
    public Vector2 FindSpawnPosition()
    {
        UpdateAroundPosition(Vector2.Zero); // Primero cargamos el mapa que rodea el punto de inicio.

        Vector2 best = Vector2.Zero;
        float bestDistance = float.MaxValue;

        foreach (var chunk in _loadedChunks.Values)
        {
            for (int lx = 0; lx < Chunk.Size; lx++)
            {
                for (int ly = 0; ly < Chunk.Size; ly++)
                {
                    if (!TileProperties.IsWalkable(chunk.Tiles[lx, ly].Type)) continue;

                    // Usamos el centro del tile como posible lugar de aparición.
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

    public IReadOnlyList<WorldObjectState> CaptureObjectStates() => ObjectStates.CaptureSnapshot();

    public void RestoreObjectStates(IEnumerable<WorldObjectState> states)
    {
        ObjectStates.RestoreSnapshot(states);

        // Si ya hay partes del mapa cargadas, aplicamos los cambios guardados ahora mismo.
        foreach (var chunk in _loadedChunks.Values)
        {
            chunk.Objects.Clear();
            PopulateObjects(chunk);
        }
    }

    private void PopulateObjects(Chunk chunk)
    {
        foreach (var spawn in _objectGenerator.Generate(chunk))
        {
            ObjectStates.TryGet(spawn.Id, out var savedState);
            var obj = new WorldObject(spawn.Id, spawn.Type, spawn.TileX, spawn.TileY,
                savedState, ObjectStates.Record);

            if (!obj.IsDestroyed && !obj.IsCollected)
                chunk.Objects.Add(obj);
        }
    }
}
