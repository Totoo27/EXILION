using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION.Entities.LivingThings;
public class LivingThing : Entity
{
    

    private int maxHealth;
    private int health;
    public bool isDead { get; private set; } = false;
    public float speed { get; private set; }

    public LivingThing(Vector2 position, Sprite sprite, int maxHealth, float speed) : base(position, sprite)
    {
        this.maxHealth = maxHealth;
        this.health = maxHealth;
        this.speed = speed;
    }

    public void Update(Vector2 mousePosition)
    {
        base.Update(mousePosition);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        base.Draw(spriteBatch, pixel);
    }

    public void takeDamage(int damage)
    {
        this.health -= damage;
        Console.WriteLine("Jugador ha recibido " + damage + ", salud actual: " + this.health + "/" + this.maxHealth);
        if (this.health <= 0)
        {
            this.health = 0;
            die();
        }
    }

    private void die()
    {
        Console.WriteLine("Living Thing ha muerto");
        isDead = true;
    }

}