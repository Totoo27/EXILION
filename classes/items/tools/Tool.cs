namespace EXILION.Items;

public class Tool : Item
{
    public float AttackSpeed { get; private set; }
    public float Damage { get; private set; }
    public float HitboxWidth { get; private set; }
    public float HitboxHeight { get; private set; }

    public Tool(int id, string name, float attackSpeed, float damage, float hitboxWidth, float hitboxHeight)
    : base(id, name, ItemType.TOOLS, true)
    {
        AttackSpeed = attackSpeed;
        Damage = damage;
        HitboxWidth = hitboxWidth;
        HitboxHeight = hitboxHeight;
    }
}
