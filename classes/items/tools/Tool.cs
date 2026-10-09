using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Items;

public class Tool : Item
{
    public float AttackSpeed { get; private set; }
    public float Damage { get; private set; }
    public float HitboxWidth { get; private set; }
    public float HitboxHeight { get; private set; }

    public Tool(int id, string name, float attackSpeed, float damage, float hitboxWidth, float hitboxHeight, Texture2D icon = null)
    : base(id, name, ItemType.TOOLS, icon, true)
    {
        AttackSpeed = attackSpeed;
        Damage = damage;
        HitboxWidth = hitboxWidth;
        HitboxHeight = hitboxHeight;
    }
}