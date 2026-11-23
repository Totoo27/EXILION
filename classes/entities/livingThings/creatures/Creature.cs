using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using EXILION.Items;
using EXILION.Saving;
using EXILION.Logic;

namespace EXILION.Entities.LivingThings;

public sealed class Creature : LivingThing
{
    #nullable enable

    private const float DespawnAfterFarSeconds = 30f;
    private const float DespawnDistanceInChunks = 3f;

    private readonly ICreatureIntelligence? intelligence;
    private readonly List<ItemStack>? dropItems;
    private float farFromPlayerTimer;
    public bool ShouldDespawn { get; private set; }
    public Guid NetworkId { get; }
    public CreatureType Type { get; private set; }
    public int ContactDamage { get; }
    public float VisualAngle { get; private set; }
    public bool CanDamagePlayer => intelligence?.CanDamagePlayer ?? false;

    public Creature(Vector2 position, ICreatureIntelligence? intelligence = null, List<ItemStack>? dropItems = null,
        int maxHealth = 25, int speed = PlayerMovementSettings.GroundSpeed / 2, CreatureType type = CreatureType.Peaceful,
        int contactDamage = 1, Point? hitboxSize = null, Guid? networkId = null)
    : base(position, maxHealth, speed, GameAudioCue.CreatureDamage, hitboxSize ?? new Point(40, 40))
    {
        this.intelligence = intelligence;
        this.dropItems = dropItems;
        Type = type;
        ContactDamage = Math.Max(0, contactDamage);
        NetworkId = networkId ?? Guid.NewGuid();
    }

    public void Update(float deltaTime, World.World world, Vector2 playerPosition, bool isNight)
    {
        base.Update(deltaTime);
        if (isDead || ShouldDespawn) return;

        float despawnDistance = world.TileSize * World.Chunk.Size * DespawnDistanceInChunks;
        if (Vector2.DistanceSquared(position, playerPosition) > despawnDistance * despawnDistance)
        {
            farFromPlayerTimer += deltaTime;
            if (farFromPlayerTimer >= DespawnAfterFarSeconds)
            {
                ShouldDespawn = true;
                return;
            }
        }
        else
        {
            farFromPlayerTimer = 0f;
        }

        intelligence?.Update(this, deltaTime, world, playerPosition, isNight);
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
        if (!isDead) VisualAngle = angle;
    }

    public void Restore(CreatureSaveData saveData)
    {
        this.position = new Vector2(saveData.PositionX, saveData.PositionY);
        this.health = Math.Clamp(saveData.Health, 1, maxHealth);
        Type = saveData.Type;
    }

    public void ApplyNetworkState(float x, float y, float rotation, int currentHealth, bool playDamageFeedback = true)
    {
        if (playDamageFeedback && currentHealth < health)
            takeDamage(health - currentHealth, new Vector2(x, y));
        position = new Vector2(x, y);
        health = Math.Clamp(currentHealth, 0, maxHealth);
        VisualAngle = rotation;
    }

    public CreatureSaveData GetSaveData()
    {
        
        return new CreatureSaveData
        {
            PositionX = this.position.X,
            PositionY = this.position.Y,

            Health = this.health,
            MaxHealth = this.maxHealth,
            Type = this.Type,
        };

    }

}
