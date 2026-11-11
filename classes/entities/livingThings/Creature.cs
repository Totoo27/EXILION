using Microsoft.Xna.Framework;

namespace EXILION.Entities.LivingThings;

public sealed class Creature : LivingThing
{
    #nullable enable

    private readonly CreatureIntelligence? intelligence;

    public Creature(Vector2 position, Sprite sprite, GameContext gameContext, CreatureIntelligence? intelligence = null,
        int maxHealth = 25, int speed = PlayerMovementSettings.GroundSpeed / 2)
    : base(position, sprite, maxHealth, speed, gameContext)
    {
        this.intelligence = intelligence;
        sprite.Update(0f, position);
    }

    public void Update(GameTime gameTime, World.World world)
    {
        if (!isDead) intelligence?.Update(this, gameTime, world);
    }

    internal void ApplyIntelligenceMovement(Vector2 direction, float deltaTime, World.World world)
    {
        if (!isDead) Move(direction, deltaTime, world);
    }

    internal void ApplyIntelligenceFacingAngle(float angle)
    {
        if (!isDead) sprite.Update(angle, position);
    }
}
