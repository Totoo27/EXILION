using Microsoft.Xna.Framework;

namespace EXILION.World;

public class ObjectDefinition
{
    public ObjectType Type { get; init; }
    public bool BlocksMovement { get; init; }   // Indica si el jugador puede atravesarlo.
    public float CollisionRadius { get; init; } // Qué tan grande es su zona de choque, medida en tiles.
    public int? MaxHealth { get; init; }
    public bool CanBeCollected { get; init; }
    public Rectangle Source { get; init; }      // Qué parte de la imagen Objects.png usamos.
    public float DrawWidthTiles { get; init; }  // Qué ancho tendrá al aparecer en el mundo.
}
