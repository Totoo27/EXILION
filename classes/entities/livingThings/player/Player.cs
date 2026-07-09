using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.Entities.LivingThings;


public class Player : LivingThing
{

    public int maxHunger { get; private set; } = 100;
    public int hunger { get; private set; }

    private KeyboardState previousKeyboardState;

    private float hungerTimer = 0f;
    public Player(Vector2 position, Sprite sprite) : base(position, sprite, 100, 2f)
    {
        this.hunger = maxHunger;
    }

    public async void Update(Vector2 mousePosition, KeyboardState currentKeyboardState, GameTime gameTime)
    {

        updateHunger(gameTime);

        if (currentKeyboardState.IsKeyDown(Keys.Left))
        {
            this.position.X -= this.speed;
        }

        if (currentKeyboardState.IsKeyDown(Keys.Right))
        {
            this.position.X += this.speed;
        }

        if (currentKeyboardState.IsKeyDown(Keys.Down))
        {
            this.position.Y += this.speed;
        }

        if (currentKeyboardState.IsKeyDown(Keys.Up))
        {
            this.position.Y -= this.speed;
        }

        if (currentKeyboardState.IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H))
        {
            Global.showHitboxes = !Global.showHitboxes;
        }

        previousKeyboardState = currentKeyboardState;
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