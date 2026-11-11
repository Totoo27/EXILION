using Microsoft.Xna.Framework;
using System;

namespace EXILION.Entities.LivingThings;

public sealed class PeacefulCreatureIntelligence : CreatureIntelligence, IPeacefulIntelligence
{
    private static readonly Random random = new();
    private Vector2 direction;
    private float directionTimer;
    private float facingAngle;

    public override void Update(Creature creature, GameTime gameTime, World.World world)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        directionTimer -= deltaTime;
        if (directionTimer <= 0f) ChooseDirection();

        MoveByIntelligence(creature, direction, deltaTime, world);
        if (direction != Vector2.Zero)
            facingAngle = MathF.Atan2(direction.Y, direction.X);
        SetFacingAngle(creature, facingAngle);
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
