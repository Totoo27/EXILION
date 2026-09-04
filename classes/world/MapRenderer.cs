using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EXILION.World;

public class MapRenderer
{
    private readonly Texture2D _tileset;
    private readonly int _tileSize;
    private readonly Dictionary<TileType, Rectangle> _sourceRects;

    public MapRenderer(Texture2D tileset, int tileSize)
    {
        _tileset = tileset;
        _tileSize = tileSize;

        _sourceRects = new Dictionary<TileType, Rectangle>
        {
            { TileType.DeepWater, new Rectangle(0 * 32, 0, 32, 32) },
            { TileType.Water,     new Rectangle(1 * 32, 0, 32, 32) },
            { TileType.Sand,      new Rectangle(2 * 32, 0, 32, 32) },
            { TileType.Grass,     new Rectangle(3 * 32, 0, 32, 32) },
            { TileType.Forest,    new Rectangle(4 * 32, 0, 32, 32) },
            { TileType.Rock,      new Rectangle(5 * 32, 0, 32, 32) },
            { TileType.Snow,      new Rectangle(6 * 32, 0, 32, 32) },
        };
    }

    public void Draw(SpriteBatch spriteBatch, World world)
    {
        foreach (var chunk in world.LoadedChunks)
        {
            int baseX = chunk.ChunkX * Chunk.Size;
            int baseY = chunk.ChunkY * Chunk.Size;

            for (int lx = 0; lx < Chunk.Size; lx++)
            {
                for (int ly = 0; ly < Chunk.Size; ly++)
                {
                    var dest = new Rectangle(
                        (baseX + lx) * _tileSize, (baseY + ly) * _tileSize,
                        _tileSize, _tileSize);

                    spriteBatch.Draw(_tileset, dest, _sourceRects[chunk.Tiles[lx, ly].Type], Color.White);
                }
            }
        }
    }
}