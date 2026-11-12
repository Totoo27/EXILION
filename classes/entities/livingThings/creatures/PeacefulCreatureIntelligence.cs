using Microsoft.Xna.Framework;
using System;

namespace EXILION.Entities.LivingThings;

public sealed class PeacefulCreatureIntelligence : ICreatureIntelligence
{
    private static readonly Random random = new();
    private Vector2 direction;
    private float directionTimer;
    private float facingAngle;
    private float fleeTimer;

    public void OnDamaged(Creature creature, Vector2 attackerPosition)
    {
        direction = creature.position - attackerPosition;
        if (direction == Vector2.Zero)
        {
            double angle = random.NextDouble() * Math.PI * 2;
            direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
        else direction.Normalize();

        directionTimer = 0.2f;
        fleeTimer = 3f;
    }

    public void Update(Creature creature, GameTime gameTime, World.World world)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        fleeTimer = Math.Max(0f, fleeTimer - deltaTime);
        directionTimer -= deltaTime;
        if (directionTimer <= 0f) ChooseDirection();

        creature.ApplyIntelligenceMovement(direction, deltaTime, world, fleeTimer > 0f ? 2f : 1f);
        if (direction != Vector2.Zero)
        {
            facingAngle = MathF.Atan2(direction.Y, direction.X);
            creature.ApplyIntelligenceFacingAngle(facingAngle);
        }
    }

    private void ChooseDirection()
    {
        directionTimer = fleeTimer > 0f
            ? 0.15f + (float)random.NextDouble() * 0.3f
            : 1.5f + (float)random.NextDouble() * 3f;
        double angle = random.NextDouble() * Math.PI * 2;
        direction = random.Next(4) == 0
            ? Vector2.Zero
            : new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
}
