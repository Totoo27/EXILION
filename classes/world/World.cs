using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using EXILION.Entities;
using EXILION.Items;

namespace EXILION.World;

public class World
{
    private readonly MapGenerator _generator;
    private readonly WorldObjectGenerator _objectGenerator;
    private readonly Dictionary<(int, int), Chunk> _loadedChunks = new();

    public int TileSize { get; }
    public int RenderDistanceChunks { get; set; } = 2;
    public WorldObjectStateStore ObjectStates { get; } = new();
    public event Action<WorldObject> ObjectDestroyed;

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

        return !IntersectsBlockingObject(worldPosition, radius);
    }

    private bool IntersectsBlockingObject(Vector2 position, float playerRadius)
    {
        foreach (var chunk in _loadedChunks.Values)
        {
            foreach (var obj in chunk.Objects)
            {
                if (obj.IsDestroyed) continue;
                var definition = obj.Definition;
                if (!definition.BlocksMovement) continue;

                float objectRadius = definition.CollisionRadius * TileSize;
                float objectX = obj.TileX * TileSize;
                float objectY = obj.TileY * TileSize;

                // The player hitbox is an axis-aligned square; find its nearest point to the object's circle.
                float nearestX = MathHelper.Clamp(objectX, position.X - playerRadius, position.X + playerRadius);
                float nearestY = MathHelper.Clamp(objectY, position.Y - playerRadius, position.Y + playerRadius);
                float dx = objectX - nearestX;
                float dy = objectY - nearestY;

                if (dx * dx + dy * dy < objectRadius * objectRadius)
                    return true;
            }
        }

        return false;
    }

    public void DamageObjects(AttackHitbox attackHitbox, int damage, Item heldItem,
        Func<WorldObjectId, bool> registerHit)
    {
        if (damage <= 0) return;

        float halfWidth = attackHitbox.Width / 2f;
        float halfHeight = attackHitbox.Height / 2f;
        float cos = MathF.Cos(attackHitbox.Rotation);
        float sin = MathF.Sin(attackHitbox.Rotation);

        foreach (var chunk in _loadedChunks.Values)
        {
            foreach (var obj in chunk.Objects)
            {
                if (obj.IsDestroyed || obj.CurrentHealth == null) continue;

                float centerX = obj.TileX * TileSize;
                float centerY = obj.TileY * TileSize;
                float deltaX = centerX - attackHitbox.Position.X;
                float deltaY = centerY - attackHitbox.Position.Y;

                // Rotate the object's center into the attack rectangle's local space.
                float localX = deltaX * cos + deltaY * sin;
                float localY = -deltaX * sin + deltaY * cos;
                float nearestX = MathHelper.Clamp(localX, -halfWidth, halfWidth);
                float nearestY = MathHelper.Clamp(localY, -halfHeight, halfHeight);
                float distanceX = localX - nearestX;
                float distanceY = localY - nearestY;
                float objectRadius = obj.Definition.CollisionRadius * TileSize;

                if (distanceX * distanceX + distanceY * distanceY > objectRadius * objectRadius)
                    continue;

                if (!registerHit(obj.Id)) continue;

                bool isAllowedTool = heldItem != null && obj.Definition.CanBeBrokenBy(heldItem);
                bool isAllowed = heldItem == null ? obj.Definition.HandCanBreak : isAllowedTool;
                obj.ApplyDamage(isAllowed ? damage : 1, allowDrops: isAllowed);
            }
        }
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

        return true; 
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
                savedState, ObjectStates.Record, obj => ObjectDestroyed?.Invoke(obj));

            if (!obj.IsDestroyed) chunk.Objects.Add(obj);
        }
    }
}
