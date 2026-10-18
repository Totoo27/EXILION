using Microsoft.Xna.Framework;

namespace EXILION.World;

public class ObjectDefinition
{
    public ObjectType Type { get; init; }
    public bool BlocksMovement { get; init; }   // ¿se puede caminar a través?
    public float CollisionRadius { get; init; } // radio de colisión, en tiles
    public Rectangle Source { get; init; }      // recorte dentro de Objects.png
    public float DrawWidthTiles { get; init; }  // ancho al dibujar, en tiles
}