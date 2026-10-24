namespace EXILION.World;

public static class TileProperties
{
    public static bool IsWalkable(TileType type)
    {
        return type switch
        {
            TileType.DeepWater => false,
            TileType.Water => false,
            _ => true, // Por ahora, cualquier otro terreno se puede caminar.
        };
    }
}
