using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using EXILION.Entities;
using EXILION.Entities.LivingThings;
using EXILION.Items;

namespace EXILION.World;

public class World
{
    private readonly MapGenerator _generator;
    private readonly WorldObjectGenerator _objectGenerator;
    private readonly Dictionary<(int, int), Chunk> _loadedChunks = new();
    private float _regrowthTimer;
    private int _regrowthCycle;
    private const float RegrowthIntervalSeconds = 60f;
    private const int RegrowthRadiusChunks = 5;

    public int TileSize { get; }
    public int RenderDistanceChunks { get; set; } = 2;
    public WorldObjectStateStore ObjectStates { get; } = new();
    public event Action<WorldObject> ObjectDestroyed;
    public event Action<WorldObjectId, ObjectType> ObjectDamaged;

    public World(int seed, int tileSize)
    {
        _generator = new MapGenerator(seed);
        _objectGenerator = new WorldObjectGenerator(seed);
        TileSize = tileSize;
    }


    public void UpdateAroundPosition(Vector2 worldPosition)
    {
        UpdateAroundPositions(new[] { worldPosition });
    }

    public void UpdateAroundPositions(IEnumerable<Vector2> positions)
    {
        var needed = new HashSet<(int, int)>();

        foreach (Vector2 worldPosition in positions)
        {
            int playerChunkX = (int)MathF.Floor(
                worldPosition.X / (TileSize * Chunk.Size)
            );

            int playerChunkY = (int)MathF.Floor(
                worldPosition.Y / (TileSize * Chunk.Size)
            );

            for (int dx = -RenderDistanceChunks; dx <= RenderDistanceChunks; dx++)
            {
                for (int dy = -RenderDistanceChunks; dy <= RenderDistanceChunks; dy++)
                {
                    needed.Add((playerChunkX + dx, playerChunkY + dy));
                }
            }
        }

        foreach (var key in needed)
        {
            if (_loadedChunks.ContainsKey(key))
                continue;

            var chunk = _generator.GenerateChunk(key.Item1, key.Item2);
            PopulateObjects(chunk);
            _loadedChunks[key] = chunk;
        }

        var toRemove = new List<(int, int)>();

        foreach (var key in _loadedChunks.Keys)
        {
            if (!needed.Contains(key))
                toRemove.Add(key);
        }

        foreach (var key in toRemove)
            _loadedChunks.Remove(key);
    }


    public void UpdateObjects(float deltaTime)
    {
        // Reserved for future timed world simulation; rendering effects are client-owned.
    }

    public void UpdateRegrowth(float deltaTime, Vector2 worldPosition)
    {
        _regrowthTimer += deltaTime;
        if (_regrowthTimer < RegrowthIntervalSeconds) return;
        _regrowthTimer %= RegrowthIntervalSeconds;

        int playerChunkX = (int)MathF.Floor(worldPosition.X / (TileSize * Chunk.Size));
        int playerChunkY = (int)MathF.Floor(worldPosition.Y / (TileSize * Chunk.Size));
        var random = new Random(unchecked(_generator.Seed ^ (++_regrowthCycle * 7919)));
        int chunkX = playerChunkX + random.Next(-RegrowthRadiusChunks, RegrowthRadiusChunks + 1);
        int chunkY = playerChunkY + random.Next(-RegrowthRadiusChunks, RegrowthRadiusChunks + 1);

        if (!_loadedChunks.TryGetValue((chunkX, chunkY), out var chunk))
        {
            chunk = _generator.GenerateChunk(chunkX, chunkY);
            PopulateObjects(chunk);
        }

        int localX = random.Next(Chunk.Size);
        int localY = random.Next(Chunk.Size);
        int tileX = chunkX * Chunk.Size + localX;
        int tileY = chunkY * Chunk.Size + localY;
        foreach (var existing in chunk.Objects)
        {
            if (!existing.IsDestroyed && !existing.IsCollected
                && (int)MathF.Floor(existing.TileX) == tileX
                && (int)MathF.Floor(existing.TileY) == tileY)
                return;
        }

        // Los índices negativos desde -2 identifican objetos regenerados y se guardan igual que los colocados.
        int spawnIndex = -1 - _regrowthCycle;
        var spawn = _objectGenerator.TryGenerateAt(chunk, localX, localY, spawnIndex, random);
        if (spawn == null) return;

        var state = new WorldObjectState(spawn.Value.Id, ObjectProperties.Get(spawn.Value.Type).MaxHealth, false, false, spawn.Value.Type);
        ObjectStates.Record(state);
        chunk.Objects.Add(new WorldObject(spawn.Value.Id, spawn.Value.Type,
            spawn.Value.TileX, spawn.Value.TileY, state, ObjectStates.Record,
            obj => ObjectDestroyed?.Invoke(obj)));

        Console.WriteLine("Regrowth" + $" at chunk ({chunkX}, {chunkY}), tile ({localX}, {localY})");
        Console.WriteLine("Player position: " + worldPosition);
    }

    public bool IsWalkable(Vector2 worldPosition, float radius = 0f)
        => IsWalkable(worldPosition, radius, false);

    public bool IsWalkable(Vector2 worldPosition, float radius, bool allowShallowWater)
    {
        // Revisamos las cuatro esquinas para que el jugador no quede medio metido en el agua.
        if (!IsTileWalkable(worldPosition.X - radius, worldPosition.Y - radius, allowShallowWater)) return false;
        if (!IsTileWalkable(worldPosition.X + radius, worldPosition.Y - radius, allowShallowWater)) return false;
        if (!IsTileWalkable(worldPosition.X - radius, worldPosition.Y + radius, allowShallowWater)) return false;
        if (!IsTileWalkable(worldPosition.X + radius, worldPosition.Y + radius, allowShallowWater)) return false;

        return !IntersectsBlockingObject(worldPosition, radius);
    }

    public bool HasObjectAtTile(Vector2 worldPosition)
    {
        int tileX = (int)MathF.Floor(worldPosition.X / TileSize);
        int tileY = (int)MathF.Floor(worldPosition.Y / TileSize);
        int chunkX = (int)MathF.Floor((float)tileX / Chunk.Size);
        int chunkY = (int)MathF.Floor((float)tileY / Chunk.Size);
        if (!_loadedChunks.TryGetValue((chunkX, chunkY), out var chunk))
        {
            chunk = _generator.GenerateChunk(chunkX, chunkY);
            PopulateObjects(chunk);
        }

        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.IsCollected) continue;
            if ((int)MathF.Floor(obj.TileX) == tileX && (int)MathF.Floor(obj.TileY) == tileY)
                return true;
        }

        return false;
    }

    public bool IsInShallowWater(Vector2 worldPosition, float radius, bool alreadySwimming)
    {
        int waterSamples = 0;
        const int samplesPerAxis = 3;
        for (int x = 0; x < samplesPerAxis; x++)
        for (int y = 0; y < samplesPerAxis; y++)
        {
            float sampleX = worldPosition.X - radius + 2f * radius * x / (samplesPerAxis - 1);
            float sampleY = worldPosition.Y - radius + 2f * radius * y / (samplesPerAxis - 1);
            if (GetTileTypeAt(sampleX, sampleY) == TileType.Water) waterSamples++;
        }

        return alreadySwimming
            ? waterSamples > 0
            : waterSamples / 9f >= PlayerMovementSettings.ShallowWaterEnterCoverage;
    }

    public bool TryPlaceBlock(Vector2 worldPosition, Vector2 playerPosition, float playerRadius, Structure item)
    {
        int tileX = (int)MathF.Floor(worldPosition.X / TileSize);
        int tileY = (int)MathF.Floor(worldPosition.Y / TileSize);
        int chunkX = (int)MathF.Floor((float)tileX / Chunk.Size);
        int chunkY = (int)MathF.Floor((float)tileY / Chunk.Size);
        if (!_loadedChunks.TryGetValue((chunkX, chunkY), out var chunk)) return false;

        int localX = tileX - chunkX * Chunk.Size;
        int localY = tileY - chunkY * Chunk.Size;
        if (!TileProperties.IsWalkable(chunk.Tiles[localX, localY].Type)) return false;

        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed) continue;
            if ((int)MathF.Floor(obj.TileX) == tileX && (int)MathF.Floor(obj.TileY) == tileY) return false;
        }

        Vector2 center = new((tileX + 0.5f) * TileSize, (tileY + 0.5f) * TileSize);
        float reach = TileSize * 4f;
        if (Vector2.DistanceSquared(center, playerPosition) > reach * reach) return false;
        float radius = ObjectProperties.Get(item.objectType).CollisionRadius * TileSize;
        if (Vector2.DistanceSquared(center, playerPosition) < (radius + playerRadius) * (radius + playerRadius)) return false;
        if (IntersectsBlockingObject(center, radius)) return false;

        // Reserve SpawnIndex -1 for player placed blocks so generated IDs and world seed stay unchanged.
        var id = new WorldObjectId(_generator.Seed, tileX, tileY, -1);
        var state = new WorldObjectState(id, ObjectProperties.Get(item.objectType).MaxHealth, false, false, item.objectType);
        ObjectStates.Record(state);
        chunk.Objects.Add(new WorldObject(id, item.objectType, tileX + 0.5f, tileY + 0.5f,
            state, ObjectStates.Record, obj => ObjectDestroyed?.Invoke(obj)));
        return true;
    }
    public bool IsOxygenSupplied(Vector2 worldPosition)
    {
        foreach (var chunk in _loadedChunks.Values)
        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.IsCollected) continue;
            float radius = obj.Definition.OxygenRadiusTiles * TileSize;
            if (radius <= 0f) continue;
            float dx = obj.TileX * TileSize - worldPosition.X;
            float dy = obj.TileY * TileSize - worldPosition.Y;
            if (dx * dx + dy * dy <= radius * radius) return true;
        }
        return false;
    }
    public bool IsNearCraftingTable(Vector2 worldPosition)
    {
        foreach (var chunk in _loadedChunks.Values)
        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.IsCollected) continue;
            if (!obj.Definition.IsCraftingTable) continue;

            float radius = obj.Definition.InteractionRadiusTiles * TileSize;
            float dx = obj.TileX * TileSize - worldPosition.X;
            float dy = obj.TileY * TileSize - worldPosition.Y;
            if (dx * dx + dy * dy <= radius * radius) return true;
        }
        return false;
    }

    public bool IsNearExit(Vector2 worldPosition)
    {
        foreach (var chunk in _loadedChunks.Values)
        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.IsCollected) continue;
            if (!obj.Definition.IsExit) continue;

            float radius = obj.Definition.InteractionRadiusTiles * TileSize;
            float dx = obj.TileX * TileSize - worldPosition.X;
            float dy = obj.TileY * TileSize - worldPosition.Y;
            if (dx * dx + dy * dy <= radius * radius) return true;
        }
        return false;
    }

    public bool IsInWater(Vector2 worldPosition)
    {
        TileType? type = GetTileTypeAt(worldPosition.X, worldPosition.Y);
        return type == TileType.Water || type == TileType.DeepWater;
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
                if (obj.ApplyDamage(isAllowed ? damage : 1, allowDrops: isAllowed))
                    ObjectDamaged?.Invoke(obj.Id, obj.Type);
            }
        }
    }

    // Busca el tile que hay debajo de este punto y comprueba si se puede caminar por él.
    private bool IsTileWalkable(float pixelX, float pixelY, bool allowShallowWater = false)
    {
        int tileX = (int)MathF.Floor(pixelX / TileSize);
        int tileY = (int)MathF.Floor(pixelY / TileSize);

        int chunkX = (int)MathF.Floor((float)tileX / Chunk.Size);
        int chunkY = (int)MathF.Floor((float)tileY / Chunk.Size);

        int localX = tileX - chunkX * Chunk.Size;
        int localY = tileY - chunkY * Chunk.Size;

        if (_loadedChunks.TryGetValue((chunkX, chunkY), out var chunk))
        {
            TileType type = chunk.Tiles[localX, localY].Type;
            return TileProperties.IsWalkable(type) || (allowShallowWater && type == TileType.Water);
        }

        return true;
    }

    private TileType? GetTileTypeAt(float pixelX, float pixelY)
    {
        int tileX = (int)MathF.Floor(pixelX / TileSize);
        int tileY = (int)MathF.Floor(pixelY / TileSize);
        int chunkX = (int)MathF.Floor((float)tileX / Chunk.Size);
        int chunkY = (int)MathF.Floor((float)tileY / Chunk.Size);
        if (!_loadedChunks.TryGetValue((chunkX, chunkY), out var chunk)) return null;
        return chunk.Tiles[tileX - chunkX * Chunk.Size, tileY - chunkY * Chunk.Size].Type;
    }

    // Encuentra un lugar seco cerca del inicio para que el jugador no aparezca en el agua.
    public Vector2 FindSpawnPosition(Vector2 preferredPosition = default)
    {
        UpdateAroundPosition(preferredPosition); // Primero cargamos el mapa alrededor del punto de referencia.

        Vector2 best = preferredPosition;
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
                    Vector2 candidate = new(px, py);
                    if (HasObjectAtTile(candidate)) continue;

                    float distance = Vector2.DistanceSquared(candidate, preferredPosition);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = candidate;
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
        foreach (var state in ObjectStates.CaptureSnapshot())
            if (state.Id.SpawnIndex <= -2)
                _regrowthCycle = Math.Max(_regrowthCycle, -state.Id.SpawnIndex - 1);

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

            if (!obj.IsDestroyed && !obj.IsCollected)
                chunk.Objects.Add(obj);
        }

        foreach (var state in ObjectStates.CaptureSnapshot())
        {
            if (state.Id.SpawnIndex > -1) continue;
            int chunkX = (int)MathF.Floor((float)state.Id.TileX / Chunk.Size);
            int chunkY = (int)MathF.Floor((float)state.Id.TileY / Chunk.Size);
            if (chunkX != chunk.ChunkX || chunkY != chunk.ChunkY || state.IsDestroyed) continue;
            var type = state.Type ?? ObjectType.OxygenGenerator;
            chunk.Objects.Add(new WorldObject(state.Id, type,
                state.Id.TileX + 0.5f, state.Id.TileY + 0.5f, state, ObjectStates.Record));
        }
    }
}
