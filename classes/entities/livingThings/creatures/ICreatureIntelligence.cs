using Microsoft.Xna.Framework;

namespace EXILION.Entities.LivingThings;

public interface ICreatureIntelligence
{
    void Update(Creature creature, GameTime gameTime, World.World world);
    void OnDamaged(Creature creature, Vector2 attackerPosition);
}
