using Microsoft.Xna.Framework;
using System;

namespace EXILION.Entities.LivingThings;

public sealed class PeacefulCreature : LivingThing
{
    private static readonly Random random = new Random();
    private Vector2 direction;
    private float directionTimer;
    private float facingAngle;

    public PeacefulCreature(Vector2 position, Sprite sprite, GameContext gameContext)
        : base(position, sprite, 50, PlayerMovementSettings.GroundSpeed / 2, gameContext)
    {
        sprite.Update(facingAngle, position);
        ChooseDirection();
    }

    public void Update(GameTime gameTime, World.World world)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        directionTimer -= dt;
        if (directionTimer <= 0f) ChooseDirection();
        Move(direction, dt, world);
        if (direction != Vector2.Zero)
            facingAngle = MathF.Atan2(direction.Y, direction.X);
        sprite.Update(facingAngle, position);
    }

    private void ChooseDirection()
    {
        directionTimer = 1.5f + (float)random.NextDouble() * 3f;
        double angle = random.NextDouble() * System.Math.PI * 2;
        direction = random.Next(4) == 0
            ? Vector2.Zero
            : new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));
    }
}
