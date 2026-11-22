using Microsoft.Xna.Framework;

namespace EXILION.Entities.LivingThings;

public interface ICreatureIntelligence
{
    bool CanDamagePlayer { get; }
    void Update(Creature creature, float deltaTime, World.World world, Vector2 playerPosition, bool isNight);
    void OnDamaged(Creature creature, Vector2 attackerPosition);
}
