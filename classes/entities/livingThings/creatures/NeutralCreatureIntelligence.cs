using Microsoft.Xna.Framework;

namespace EXILION.Entities.LivingThings;

public sealed class NeutralCreatureIntelligence : ICreatureIntelligence
{
    public void Update(Creature creature, GameTime gameTime, World.World world) { }

    public void OnDamaged(Creature creature, Vector2 attackerPosition) { }
}
