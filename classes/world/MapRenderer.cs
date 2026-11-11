using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EXILION.World;

public class MapRenderer
{
    private const int ObjectSourceSize = 64;

    private readonly Texture2D _tileset;
    private readonly Texture2D _objectsTexture;
    private readonly Texture2D _oxygenGeneratorTexture;
    private readonly Texture2D _blockTexture;
    private readonly int _tileSize;
    private readonly Dictionary<TileType, Rectangle> _sourceRects;
    private readonly List<WorldObject> _visibleObjects = new();

    public MapRenderer(Texture2D tileset, Texture2D objectsTexture, Texture2D oxygenGeneratorTexture,
        Texture2D blockTexture, int tileSize)
    {
        _tileset = tileset;
        _objectsTexture = objectsTexture;
        _oxygenGeneratorTexture = oxygenGeneratorTexture;
        _blockTexture = blockTexture;
        _tileSize = tileSize;

        _sourceRects = new Dictionary<TileType, Rectangle>
        {
            { TileType.DeepWater, new Rectangle(0, 0, 32, 32) },
            { TileType.Water, new Rectangle(32, 0, 32, 32) },
            { TileType.Sand, new Rectangle(64, 0, 32, 32) },
            { TileType.Grass, new Rectangle(96, 0, 32, 32) },
            { TileType.Forest, new Rectangle(128, 0, 32, 32) },
            { TileType.Rock, new Rectangle(160, 0, 32, 32) },
            { TileType.Snow, new Rectangle(192, 0, 32, 32) },
        };
    }

    public void Draw(SpriteBatch spriteBatch, World world)
    {
        foreach (var chunk in world.LoadedChunks)
        {
            int baseX = chunk.ChunkX * Chunk.Size;
            int baseY = chunk.ChunkY * Chunk.Size;

            for (int lx = 0; lx < Chunk.Size; lx++)
            for (int ly = 0; ly < Chunk.Size; ly++)
            {
                var destination = new Rectangle(
                    (baseX + lx) * _tileSize, (baseY + ly) * _tileSize, _tileSize, _tileSize);
                spriteBatch.Draw(_tileset, destination, _sourceRects[chunk.Tiles[lx, ly].Type], Color.White);
            }
        }
    }

    public void DrawObjects(SpriteBatch spriteBatch, World world, bool drawAbove)
    {
        _visibleObjects.Clear();
        foreach (var chunk in world.LoadedChunks)
        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.IsCollected) continue;
            if (obj.Definition.AbovePlayer != drawAbove) continue;
            _visibleObjects.Add(obj);
        }

        _visibleObjects.Sort((a, b) => a.TileY.CompareTo(b.TileY));

        foreach (var obj in _visibleObjects)
        {
            var definition = obj.Definition;
            Texture2D texture = obj.Type switch
            {
                _ => _objectsTexture
            };

            Rectangle? source = obj.Type switch
            {
                _ => definition.Source
            };

            Vector2 center = ObjectCenter(obj);
            float width = definition.DrawWidthTiles * _tileSize;
            float height = width * source.Value.Height / source.Value.Width;
            float scale = obj.DrawScale;
            var destination = new Rectangle(
                (int)(center.X - width * scale / 2f),
                (int)(center.Y - height * scale / 2f),
                (int)(width * scale),
                (int)(height * scale));

            spriteBatch.Draw(texture, destination, source, Color.White);
        }
    }

    public void DrawOxygenRanges(SpriteBatch spriteBatch, World world, Texture2D pixel)
    {
        const int segments = 96;
        float thickness = Math.Max(1.5f, _tileSize * 0.0525f);

        foreach (var chunk in world.LoadedChunks)
        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.Definition.OxygenRadiusTiles <= 0f) continue;

            float radius = obj.Definition.OxygenRadiusTiles * _tileSize;
            Vector2 center = ObjectCenter(obj);
            Color color = obj.Definition.OxygenCircleColor * (obj.Definition.OxygenCircleAlpha / 255f);
            for (int i = 0; i < segments; i++)
            {
                float a0 = MathHelper.TwoPi * i / segments;
                float a1 = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 start = center + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;
                Vector2 end = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;
                Vector2 edge = end - start;
                spriteBatch.Draw(pixel, start, null, color, MathF.Atan2(edge.Y, edge.X), Vector2.Zero,
                    new Vector2(edge.Length(), thickness), SpriteEffects.None, 0f);
            }
        }
    }

    public void DrawObjectHitboxes(SpriteBatch spriteBatch, World world, Texture2D pixel)
    {
        const int segments = 32;
        float thickness = Math.Max(1f, _tileSize * 0.025f);

        foreach (var chunk in world.LoadedChunks)
        foreach (var obj in chunk.Objects)
        {
            if (obj.IsDestroyed || obj.IsCollected) continue;

            float radius = obj.Definition.CollisionRadius * _tileSize;
            if (radius <= 0f) continue;

            Vector2 center = ObjectCenter(obj);
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

    private Vector2 ObjectCenter(WorldObject obj) => new(obj.TileX * _tileSize, obj.TileY * _tileSize);

    private static Rectangle GetSquareSource(Texture2D texture)
    {
        int size = Math.Min(ObjectSourceSize, Math.Min(texture.Width, texture.Height));
        return new Rectangle(0, 0, size, size);
    }
}
