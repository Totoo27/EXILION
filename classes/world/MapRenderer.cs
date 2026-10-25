using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
  
    public void DrawObjects(SpriteBatch spriteBatch, World world, bool drawAbove)
    {
        _visibleObjects.Clear();

        foreach (var chunk in world.LoadedChunks)
        {
            foreach (var obj in chunk.Objects)
            {
                if(obj.IsDestroyed) continue;
                if(!drawAbove && obj.Definition.AbovePlayer) continue;
                if(drawAbove && !obj.Definition.AbovePlayer) continue;
                _visibleObjects.Add(obj);
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

            var dest = new Rectangle((int)(footX - width / 2f), (int)(footY - height / 2f), (int)width, (int)height);
            spriteBatch.Draw(_objectsTexture, dest, def.Source, Color.White);
        }
    }

    public void DrawObjectHitboxes(SpriteBatch spriteBatch, World world, Texture2D pixel)
    {
        const int segments = 32;
        float thickness = Math.Max(1f, _tileSize * 0.025f);

        foreach (var chunk in world.LoadedChunks)
        {
            foreach (var obj in chunk.Objects)
            {
                if (obj.IsDestroyed) continue;
                float radius = obj.Definition.CollisionRadius * _tileSize;
                if (radius <= 0f) continue;

                Vector2 center = new Vector2(obj.TileX * _tileSize, obj.TileY * _tileSize);
                Color color = obj.Definition.BlocksMovement ? Color.Red : Color.Yellow;

                for (int i = 0; i < segments; i++)
                {
                    float startAngle = MathHelper.TwoPi * i / segments;
                    float endAngle = MathHelper.TwoPi * (i + 1) / segments;
                    Vector2 start = center + new Vector2(MathF.Cos(startAngle), MathF.Sin(startAngle)) * radius;
                    Vector2 end = center + new Vector2(MathF.Cos(endAngle), MathF.Sin(endAngle)) * radius;
                    Vector2 edge = end - start;

                    spriteBatch.Draw(pixel, start, null, color, MathF.Atan2(edge.Y, edge.X), Vector2.Zero,
                        new Vector2(edge.Length(), thickness), SpriteEffects.None, 0f);
                }
            }
        }
    }
}
