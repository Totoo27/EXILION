using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using EXILION.Entities.LivingThings;

namespace EXILION.World;

/// <summary>Genera criaturas proceduralmente sobre los chunks que el mundo tiene cargados.</summary>
public sealed class CreatureSpawnGenerator
{
    private const float SpawnInterval = 6f;
    private const int MaxCreatures = 20;
    private const float SpawnChancePerAttempt = 0.75f;

    private readonly World world;
    private readonly Random random = new();
    private float spawnTimer;

    public CreatureSpawnGenerator(World world)
    {
        this.world = world;
    }

    public Creature TrySpawn(float deltaTime, Vector2 playerPosition, bool isNight,
        IReadOnlyList<Creature> activeCreatures, GameContext gameContext)
    {
        spawnTimer += deltaTime;
        if (spawnTimer < SpawnInterval) return null;
        spawnTimer %= SpawnInterval;

        if (activeCreatures.Count(creature => !creature.isDead) >= MaxCreatures || random.NextDouble() >= SpawnChancePerAttempt) return null;

        Chunk[] loadedChunks = world.LoadedChunks.ToArray();
        if (loadedChunks.Length == 0) return null;

        // Buscar un tile caminable de los chunks cargados, evitando aparecer encima del jugador.
        for (int attempt = 0; attempt < 32; attempt++)
        {
            Chunk chunk = loadedChunks[random.Next(loadedChunks.Length)];
            int localX = random.Next(Chunk.Size);
            int localY = random.Next(Chunk.Size);
            TileType biome = chunk.Tiles[localX, localY].Type;
            if (!TileProperties.IsWalkable(biome)) continue;

            Vector2 position = new Vector2(
                (chunk.ChunkX * Chunk.Size + localX + 0.5f) * world.TileSize,
                (chunk.ChunkY * Chunk.Size + localY + 0.5f) * world.TileSize);
            if (Vector2.DistanceSquared(position, playerPosition) < world.TileSize * world.TileSize * 9f) continue;
            if (!world.IsWalkable(position, gameContext.ScaleX(20))) continue;
            if (activeCreatures.Any(creature => !creature.isDead
                && Vector2.DistanceSquared(position, creature.position) < world.TileSize * world.TileSize * 4f))
                continue;

            return CreatureFactory.CreateCreature(ChooseCreatureType(biome, isNight), position, gameContext);
        }

        return null;
    }

    private CreatureType ChooseCreatureType(TileType biome, bool isNight)
    {
        double roll = random.NextDouble();
        bool isRock = biome == TileType.Rock;

        if (isNight)
        {
            if (roll < 0.35) return CreatureType.Hostile;
            roll -= 0.35;
        }

        // Las zonas de roca favorecen criaturas neutrales; el resto favorece pacíficas.
        double neutralChance = isRock ? 0.65 : 0.30;
        return roll < neutralChance ? CreatureType.Neutral : CreatureType.Peaceful;
    }
}
