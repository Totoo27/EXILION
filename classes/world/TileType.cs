public enum TileType
{
    DeepWater, Water, Sand, Grass, Forest, Rock, Snow
}

public struct MapTile
{
    public TileType Type;
    public float Elevation;
    public float Moisture;
}