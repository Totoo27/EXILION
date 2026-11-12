using Microsoft.Xna.Framework;
using System;

namespace EXILION.Entities.LivingThings;

public sealed class HostileCreatureIntelligence : ICreatureIntelligence
{
    public void Update(Creature creature, GameTime gameTime, World.World world, Vector2 playerPosition)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 direction = playerPosition - creature.position;
        if (direction != Vector2.Zero)
        {
            direction.Normalize();
            creature.ApplyIntelligenceMovement(direction, deltaTime, world);
            creature.ApplyIntelligenceFacingAngle(MathF.Atan2(direction.Y, direction.X));
        }
    }

    public void OnDamaged(Creature creature, Vector2 attackerPosition) { }
}
