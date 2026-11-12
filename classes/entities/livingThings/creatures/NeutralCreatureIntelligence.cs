using Microsoft.Xna.Framework;
using System;

namespace EXILION.Entities.LivingThings;

public sealed class NeutralCreatureIntelligence : ICreatureIntelligence
{
    private static readonly Random random = new();
    private Vector2 direction;
    private float directionTimer;
    private float facingAngle;
    private bool attackMode = false;

    public void OnDamaged(Creature creature, Vector2 attackerPosition)
    {
        attackMode = true;
        direction = creature.position - attackerPosition;
        if (direction == Vector2.Zero)
        {
            double angle = random.NextDouble() * Math.PI * 2;
            direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
        else direction.Normalize();

        directionTimer = 0.2f;
    }

    public void Update(Creature creature, GameTime gameTime, World.World world, Vector2 playerPosition)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (attackMode)
        {
            direction = playerPosition - creature.position;
            if (direction != Vector2.Zero) direction.Normalize();
            creature.ApplyIntelligenceMovement(direction, deltaTime, world);
            if (direction != Vector2.Zero)
            {
                facingAngle = MathF.Atan2(direction.Y, direction.X);
                creature.ApplyIntelligenceFacingAngle(facingAngle);
            }
            return;
        }

        directionTimer -= deltaTime;
        if (directionTimer <= 0f) ChooseDirection();

        creature.ApplyIntelligenceMovement(direction, deltaTime, world, 1f);
        if (direction != Vector2.Zero)
        {
            facingAngle = MathF.Atan2(direction.Y, direction.X);
            creature.ApplyIntelligenceFacingAngle(facingAngle);
        }
    }

    private void ChooseDirection()
    {
        directionTimer = 1.5f + (float)random.NextDouble() * 3f;
        double angle = random.NextDouble() * Math.PI * 2;
        direction = random.Next(4) == 0
            ? Vector2.Zero
            : new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
}
