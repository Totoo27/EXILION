using Microsoft.Xna.Framework;
using System.Collections.Generic;
using EXILION.Items;

namespace EXILION.Entities.LivingThings;

public sealed class Creature : LivingThing
{
    #nullable enable

    private readonly ICreatureIntelligence? intelligence;
    private readonly List<ItemStack>? dropItems;

    public Creature(Vector2 position, Sprite sprite, GameContext gameContext, ICreatureIntelligence? intelligence = null, List<ItemStack>? dropItems = null, 
        int maxHealth = 25, int speed = PlayerMovementSettings.GroundSpeed / 2)
    : base(position, sprite, maxHealth, speed, gameContext, Assets.SoundEffects.creatureDamage)
    {
        this.intelligence = intelligence;
        this.dropItems = dropItems;
        sprite.Update(0f, position);
    }

    public void Update(GameTime gameTime, World.World world)
    {
        base.Update(gameTime);
        if (!isDead) intelligence?.Update(this, gameTime, world);
    }

    public IReadOnlyList<ItemStack>? GetDropItems() => dropItems;

    public override void takeDamage(int damage)
    {
        takeDamage(damage, position);
    }

    public void takeDamage(int damage, Vector2 attackerPosition)
    {
        bool wasAlive = !isDead;
        base.takeDamage(damage);
        if (wasAlive && !isDead) intelligence?.OnDamaged(this, attackerPosition);
    }

    internal void ApplyIntelligenceMovement(Vector2 direction, float deltaTime, World.World world, float speedMultiplier = 1f)
    {
        if (!isDead) Move(direction, deltaTime, world, speedMultiplier);
    }

    internal void ApplyIntelligenceFacingAngle(float angle)
    {
        if (!isDead) sprite.Update(angle, position);
    }
}
