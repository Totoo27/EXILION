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
            { TileType.DeepWater, new Rectangle(0 * tileSize, 0, tileSize, tileSize) },
            { TileType.Water,     new Rectangle(1 * tileSize, 0, tileSize, tileSize) },
            { TileType.Sand,      new Rectangle(2 * tileSize, 0, tileSize, tileSize) },
            { TileType.Grass,     new Rectangle(3 * tileSize, 0, tileSize, tileSize) },
            { TileType.Forest,    new Rectangle(4 * tileSize, 0, tileSize, tileSize) },
            { TileType.Rock,      new Rectangle(5 * tileSize, 0, tileSize, tileSize) },
            { TileType.Snow,      new Rectangle(6 * tileSize, 0, tileSize, tileSize) },
        };
    }

    public void Draw(SpriteBatch spriteBatch, MapTile[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var dest = new Rectangle(x * _tileSize, y * _tileSize, _tileSize, _tileSize);
                spriteBatch.Draw(_tileset, dest, _sourceRects[map[x, y].Type], Color.White);
            }
        }
    }
}