using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.Entities.LivingThings;

public class Player : LivingThing
{


    private int maxStat = 10;
    private int maxOxygen = 5;

    private PlayerStat oxygen;
    private PlayerStat hunger;
    private PlayerStat thirst;


    public Player(Vector2 position, Sprite sprite, GameContext gameContext)
    : base(position, sprite, 100, (float) gameContext.ScaleXY(3), gameContext)
    {

        hunger = new PlayerStat();
        thirst = new PlayerStat();
        oxygen = new PlayerStat();

        hunger.max = maxStat;
        thirst.max = maxStat;
        oxygen.max = maxOxygen;

        hunger.value = hunger.max;
        thirst.value = thirst.max;
        oxygen.value = oxygen.max;
    }

    public async void Update(Vector2 mousePosition, InputManager input, GameTime gameTime)
    {

        hunger = updateStat(gameTime, hunger);
        thirst = updateStat(gameTime, thirst);
        oxygen = updateStat(gameTime, oxygen);

        float currentSpeed = this.speed;

        if(input.IsKeyHeld(Keys.LeftShift))
        {
            currentSpeed *= 2;
        }

        if (input.IsKeyHeld(Keys.Left))
        {
            this.position.X -= currentSpeed;
        }

        if (input.IsKeyHeld(Keys.Right))
        {
            this.position.X += currentSpeed;
        }

        if (input.IsKeyHeld(Keys.Down))
        {
            this.position.Y += currentSpeed;
        }

        if (input.IsKeyHeld(Keys.Up))
        {
            this.position.Y -= currentSpeed;
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

    public PlayerStat updateStat(GameTime gameTime, PlayerStat stat)
    {

        stat.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (stat.timer >= 1f)
        {
            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
            }
        }

        return stat;

    }

}