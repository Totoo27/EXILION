using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Entities.LivingThings;
public abstract class LivingThing : Entity
{
    #nullable enable
    
    public int maxHealth { get; private set; }
    protected int health;
    public bool isDead { get; private set; } = false;
    public int speed { get; private set; }
    public event Action? deathEvent;

    public LivingThing(Vector2 position, Sprite sprite, int maxHealth, int speed, GameContext gameContext) : base(position, sprite, gameContext)
    {
        this.maxHealth = maxHealth;
        this.health = maxHealth;
        this.speed = speed;
    }

    public virtual void takeDamage(int damage)
    {
        this.health -= damage;
        if (this.health <= 0)
        {
            this.health = 0;
            die();
        }
    }

    private void die()
    {
        isDead = true;
        deathEvent?.Invoke();
    }

}