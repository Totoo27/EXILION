using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EXILION.World;

public class MapRenderer
{
    private readonly Texture2D _tileset;
    private readonly Texture2D _objectsTexture;
    private readonly int _tileSize;
    private readonly Dictionary<TileType, Rectangle> _sourceRects;
    private readonly List<WorldObject> _visibleObjects = new();

    public MapRenderer(Texture2D tileset, Texture2D objectsTexture, int tileSize)
    {
        _tileset = tileset;
        _objectsTexture = objectsTexture;
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

    // Dibujamos primero lo que queda detrás del jugador y después lo que queda delante.
    public void DrawObjects(SpriteBatch spriteBatch, World world, float playerY, bool inFrontOfPlayer)
    {
        _visibleObjects.Clear();

        foreach (var chunk in world.LoadedChunks)
        {
            foreach (var obj in chunk.Objects)
            {
                bool isInFront = obj.TileY * _tileSize > playerY;
                if (isInFront == inFrontOfPlayer) _visibleObjects.Add(obj);
            }
        }

        // Al dibujar de arriba hacia abajo, los objetos más bajos pueden tapar a los otros.
        _visibleObjects.Sort((a, b) => a.TileY.CompareTo(b.TileY));

        foreach (var obj in _visibleObjects)
        {
            var def = obj.Definition;

            float footX = obj.TileX * _tileSize;
            float footY = obj.TileY * _tileSize;

            float width = def.DrawWidthTiles * _tileSize;
            float height = width * def.Source.Height / def.Source.Width;

            // Tomamos este punto como los pies del objeto y apoyamos ahí el dibujo.
            var dest = new Rectangle((int)(footX - width / 2f), (int)(footY - height), (int)width, (int)height);
            spriteBatch.Draw(_objectsTexture, dest, def.Source, Color.White);
        }
    }
}
