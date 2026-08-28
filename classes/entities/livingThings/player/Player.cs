using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EXILION.Entities.LivingThings;

public class Player : LivingThing
{


    public int maxStat = 100;
    private int maxOxygen = 100;

    public PlayerStat oxygen {get; private set;}
    public PlayerStat hunger {get; private set;}
    public PlayerStat thirst {get; private set;}

    public event Action<int>? OxygenChanged;
    public event Action<int>? HungerChanged;
    public event Action<int>? ThirstChanged;


    public Player(Vector2 position, Sprite sprite, GameContext gameContext)
    : base(position, sprite, 100, (float) gameContext.ScaleXY(3), gameContext)
    {

        hunger = new PlayerStat(maxStat);
        thirst = new PlayerStat(maxStat);
        oxygen = new PlayerStat(maxOxygen);
    }

    public async void Update(Vector2 mousePosition, InputManager input, GameTime gameTime)
    {

        updateHunger(gameTime);
        updateThirst(gameTime);
        updateOxygen(gameTime);

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

    public void updateHunger(GameTime gameTime)
    {

        PlayerStat stat = hunger;

        stat.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (stat.timer >= 1f)
        {
            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
                hunger = stat;
                return;
            }

            stat.timer = 0f;
            stat.value--;
            HungerChanged?.Invoke(stat.value);

        }

        hunger = stat;


    }

    public void updateOxygen(GameTime gameTime)
    {

        PlayerStat stat = oxygen;

        stat.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (stat.timer >= 1f)
        {
            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
                oxygen = stat;
                return;
            }

            stat.timer = 0f;
            stat.value--;
            OxygenChanged?.Invoke(stat.value);

        }

        oxygen = stat;

    }

    public void updateThirst(GameTime gameTime)
    {

        PlayerStat stat = thirst;

        stat.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (stat.timer >= 1f)
        {
            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
                thirst = stat;
                return;
            }

            stat.timer = 0f;
            stat.value--;
            ThirstChanged?.Invoke(stat.value);

        }

        thirst = stat;

    }

}