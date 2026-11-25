using System.Collections.Generic;
using System;

namespace EXILION.World;
public sealed class WorldObjectGenerator
{
    private readonly int _seed;

    public WorldObjectGenerator(int seed) => _seed = seed;

    public List<WorldObjectSpawn> Generate(Chunk chunk)
    {
        var objects = new List<WorldObjectSpawn>();
        int baseX = chunk.ChunkX * Chunk.Size;
        int baseY = chunk.ChunkY * Chunk.Size;

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int y = 0; y < Chunk.Size; y++)
            {
                int worldX = baseX + x;
                int worldY = baseY + y;
                var rules = ObjectProperties.GetSpawnRules(chunk.Tiles[x, y].Type);

                for (int i = 0; i < rules.Length; i++)
                {
                    if (WorldHash.Value01(_seed, worldX, worldY, 100 + i) >= rules[i].Chance)
                        continue;

                    var id = new WorldObjectId(_seed, worldX, worldY, i);
                    objects.Add(new WorldObjectSpawn(id, rules[i].Type, worldX + 0.5f, worldY + 0.5f));
                    break;
                }
            }
        }

        return objects;
    }

    public WorldObjectSpawn? TryGenerateAt(Chunk chunk, int localX, int localY, int spawnIndex, Random random)
    {
        var rules = ObjectProperties.GetSpawnRules(chunk.Tiles[localX, localY].Type);
        for (int i = 0; i < rules.Length; i++)
        {
            if (random.NextDouble() >= rules[i].Chance) continue;
            int worldX = chunk.ChunkX * Chunk.Size + localX;
            int worldY = chunk.ChunkY * Chunk.Size + localY;
            var id = new WorldObjectId(_seed, worldX, worldY, spawnIndex);
            return new WorldObjectSpawn(id, rules[i].Type, worldX + 0.5f, worldY + 0.5f);
        }

        return null;
    }
}
