using Microsoft.Xna.Framework;

public class MapGenerator
{
    private readonly int _width, _height;
    private readonly float _elevOffsetX, _elevOffsetY;
    private readonly float _moistOffsetX, _moistOffsetY;

    public MapGenerator(int width, int height, int seed)
    {
        _width = width;
        _height = height;

        var rng = new System.Random(seed);
        _elevOffsetX = (float)rng.NextDouble() * 10000f;
        _elevOffsetY = (float)rng.NextDouble() * 10000f;
        _moistOffsetX = (float)rng.NextDouble() * 10000f;
        _moistOffsetY = (float)rng.NextDouble() * 10000f;
    }

    public MapTile[,] Generate()
    {
        var map = new MapTile[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float elevation = SimplexNoise.Fractal(
                    x + _elevOffsetX, y + _elevOffsetY,
                    octaves: 5, persistence: 0.5f, scale: 0.03f);

                float moisture = SimplexNoise.Fractal(
                    x + _moistOffsetX, y + _moistOffsetY,
                    octaves: 4, persistence: 0.5f, scale: 0.05f);

                // Normalizar de [-1,1] a [0,1]
                elevation = (elevation + 1f) / 2f;
                moisture = (moisture + 1f) / 2f;

                // Falloff radial opcional: hace que los bordes del mapa
                // tiendan a agua (islas). Sacalo si querés terreno infinito/continuo.
                float distX = (x / (float)_width) - 0.5f;
                float distY = (y / (float)_height) - 0.5f;
                float dist = (float)System.Math.Sqrt(distX * distX + distY * distY) * 2f;
                elevation -= MathHelper.Clamp(dist - 0.4f, 0f, 1f) * 0.8f;

                map[x, y] = new MapTile
                {
                    Elevation = elevation,
                    Moisture = moisture,
                    Type = ClassifyBiome(elevation, moisture)
                };
            }
        }

        return map;
    }

    private TileType ClassifyBiome(float elevation, float moisture)
    {
        if (elevation < 0.30f) return TileType.DeepWater;
        if (elevation < 0.38f) return TileType.Water;
        if (elevation < 0.42f) return TileType.Sand;
        if (elevation > 0.85f) return TileType.Snow;
        if (elevation > 0.70f) return TileType.Rock;

        // Zona "media": el bioma depende de la humedad
        if (moisture < 0.3f) return TileType.Sand;
        if (moisture < 0.6f) return TileType.Grass;
        return TileType.Forest;
    }
}