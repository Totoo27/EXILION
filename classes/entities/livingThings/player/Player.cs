using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION;

public class Player : LivingThing
{

    public int maxHunger { get; private set; } = 100;
    public int hunger { get; private set; }

    private float hungerTimer = 0f;
    public Player(Vector2 position, Sprite sprite) : base(position, sprite, 100, 2f)
    {
        this.hunger = maxHunger;
    }

    public async void Update(Vector2 mousePosition, KeyboardState keyboardState, GameTime gameTime)
    {

        updateHunger(gameTime);

        if (keyboardState.IsKeyDown(Keys.Left))
        {
            this.position.X -= this.speed;
        }

        if (keyboardState.IsKeyDown(Keys.Right))
        {
            this.position.X += this.speed;
        }

        if (keyboardState.IsKeyDown(Keys.Down))
        {
            this.position.Y += this.speed;
        }

        if (keyboardState.IsKeyDown(Keys.Up))
        {
            this.position.Y -= this.speed;
        }

        if (keyboardState.IsKeyDown(Keys.Space))
        {
            this.takeDamage(1);
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