using System.Collections.Generic;

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
                    float offsetX = 0.2f + 0.6f * WorldHash.Value01(_seed, worldX, worldY, 200);
                    float offsetY = 0.2f + 0.6f * WorldHash.Value01(_seed, worldX, worldY, 201);
                    objects.Add(new WorldObjectSpawn(id, rules[i].Type, worldX + offsetX, worldY + offsetY));
                    break; // Por ahora solo aparece un objeto en cada tile.
                }
            }
        }

        return objects;
    }
}
