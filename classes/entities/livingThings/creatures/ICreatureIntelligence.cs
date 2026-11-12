using Microsoft.Xna.Framework;

namespace EXILION.Entities.LivingThings;

public interface ICreatureIntelligence
{
    void Update(Creature creature, GameTime gameTime, World.World world, Vector2 playerPosition);
    void OnDamaged(Creature creature, Vector2 attackerPosition);
}
