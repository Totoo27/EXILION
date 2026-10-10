using Microsoft.Xna.Framework;

namespace EXILION.Entities;

public struct AttackHitbox
{
    public Vector2 Position { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Rotation { get; set; }

    public AttackHitbox(Vector2 position, float width, float height, float rotation)
    {
        Position = position;
        Width = width;
        Height = height;
        Rotation = rotation;
    }
}