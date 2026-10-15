namespace EXILION.World;

public class WorldObject
{
    public ObjectType Type { get; }
    public float TileX { get; } // posición global en unidades de tile (con decimales)
    public float TileY { get; }
    public ObjectDefinition Definition => ObjectProperties.Get(Type);

    public WorldObject(ObjectType type, float tileX, float tileY)
    {
        Type = type;
        TileX = tileX;
        TileY = tileY;
    }
}