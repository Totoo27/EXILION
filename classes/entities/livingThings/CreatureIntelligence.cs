using Microsoft.Xna.Framework;

namespace EXILION.Entities.LivingThings;

public abstract class CreatureIntelligence : ICreatureIntelligence
{
    public abstract void Update(Creature creature, GameTime gameTime, World.World world);

    protected void MoveByIntelligence(Creature creature, Vector2 direction, float deltaTime, World.World world)
    {
        creature.ApplyIntelligenceMovement(direction, deltaTime, world);
    }

    protected void SetFacingAngle(Creature creature, float angle)
    {
        creature.ApplyIntelligenceFacingAngle(angle);
    }
}

public interface ICreatureIntelligence
{
}

public interface IPeacefulIntelligence : ICreatureIntelligence
{
}

public interface INeutralIntelligence : ICreatureIntelligence
{
}

public interface IAggressiveIntelligence : ICreatureIntelligence
{
}
