using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.Entities.LivingThings;


public class Player : LivingThing
{

    public int maxHunger { get; private set; } = 100;
    public int hunger { get; private set; }

    private float hungerTimer = 0f;

    public Player(Vector2 position, Sprite sprite, GameContext gameContext)
    : base(position, sprite, 100, (float) gameContext.ScaleXY(3), gameContext)
    {
        this.hunger = maxHunger;
    }

    // NUEVO: recibe el mundo para poder preguntarle si puede avanzar
    public async void Update(Vector2 mousePosition, InputManager input, GameTime gameTime, EXILION.World.World world)
    {

        updateHunger(gameTime);

        float currentSpeed = this.speed;
        float collisionRadius = hitbox.Width / 2f; // NUEVO

        if(input.IsKeyHeld(Keys.LeftShift))
        {
            currentSpeed *= 2;
        }

        // NUEVO: primero se calcula cuánto querés moverte...
        Vector2 delta = Vector2.Zero;

        if (input.IsKeyHeld(Keys.Left))  delta.X -= currentSpeed;
        if (input.IsKeyHeld(Keys.Right)) delta.X += currentSpeed;
        if (input.IsKeyHeld(Keys.Down))  delta.Y += currentSpeed;
        if (input.IsKeyHeld(Keys.Up))    delta.Y -= currentSpeed;

        // NUEVO: ...y después se mueve cada eje solo si ese lugar es caminable
        Vector2 nextX = new Vector2(this.position.X + delta.X, this.position.Y);
        if (world == null || world.IsWalkable(nextX, collisionRadius))
        {
            this.position.X = nextX.X;
        }

        Vector2 nextY = new Vector2(this.position.X, this.position.Y + delta.Y);
        if (world == null || world.IsWalkable(nextY, collisionRadius))
        {
            this.position.Y = nextY.Y;
        }

        if (input.IsKeyPressed(Keys.H))
        {
            gameContext.showHitboxes = !gameContext.showHitboxes;
        }

        base.Update(mousePosition);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        base.Draw(spriteBatch, pixel);
    }

    public void updateHunger(GameTime gameTime)
    {

        hungerTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (hungerTimer >= 1f)
        {
            hungerTimer -= 1f;
            hunger--;

            Console.WriteLine($"Hunger: {hunger}");

            if (hunger <= 0)
            {
                takeDamage(1);
            }
        }

    }

}