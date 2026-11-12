using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using EXILION.World;

namespace EXILION.Entities.LivingThings;
public abstract class LivingThing : Entity
{
    #nullable enable
    
    public int maxHealth { get; private set; }
    protected int health;
    public bool isDead { get; private set; } = false;
    public int speed { get; private set; }
    public event Action? deathEvent;
    public bool IsSwimming { get; private set; }
    private float movementSpeedScale = 1f;
    protected SoundEffect damageSoundEffect;

    protected readonly Color damageColor = new Color(230, 180, 180);
    private float damagedTimer = 0f; 

    public LivingThing(Vector2 position, Sprite sprite, int maxHealth, int speed, GameContext gameContext, SoundEffect damageSoundEffect) : base(position, sprite, gameContext)
    {
        this.maxHealth = maxHealth;
        this.health = maxHealth;
        this.speed = speed;
        this.damageSoundEffect = damageSoundEffect;
    }

    protected void Move(Vector2 direction, float deltaTime, World.World? world, float speedMultiplier = 1f)
    {
        float radius = hitbox.Width / 2f;
        IsSwimming = world != null && world.IsInShallowWater(position, radius, IsSwimming);
        float targetScale = IsSwimming ? PlayerMovementSettings.ShallowWaterSpeedMultiplier : 1f;
        movementSpeedScale += MathHelper.Clamp(targetScale - movementSpeedScale,
            -PlayerMovementSettings.SpeedTransitionRate * deltaTime,
            PlayerMovementSettings.SpeedTransitionRate * deltaTime);

        if (direction != Vector2.Zero) direction.Normalize();
        Vector2 delta = direction * speed * speedMultiplier * movementSpeedScale * deltaTime;
        Vector2 next = new Vector2(position.X + delta.X, position.Y);
        if (world == null || world.IsWalkable(next, radius, true)) position.X = next.X;
        next = new Vector2(position.X, position.Y + delta.Y);
        if (world == null || world.IsWalkable(next, radius, true)) position.Y = next.Y;
    }

    public virtual void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Damage timer

        if (damagedTimer > 0f)
        {
            damagedTimer -= deltaTime;

            if (damagedTimer <= 0f)
            {
                color = Color.White;
            }
        }
    }

    public virtual void takeDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        this.health -= damage;

        color = damageColor;
        damagedTimer = 0.3f;

        if (this.health <= 0)
        {
            this.health = 0;
            die();
        }

        SFX.Play(damageSoundEffect);

    }

    private void die()
    {
        isDead = true;
        deathEvent?.Invoke();
    }

}
