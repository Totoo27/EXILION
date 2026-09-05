using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Items;
using EXILION.Entities.CatchableItems;

namespace EXILION.Entities.LivingThings;

public class Player : LivingThing
{

    public const int maxStat = 100;
    private int maxOxygen = 100;

    private float damagedTimer = 0f;
    private float runningMultiplier = 1f;

    private Color damageColor = new Color(230, 180, 180);

    public PlayerStat oxygen {get; private set;}
    public PlayerStat hunger {get; private set;}
    public PlayerStat thirst {get; private set;}


    public event Action<int>? OxygenChanged;
    public event Action<int>? HungerChanged;
    public event Action<int>? ThirstChanged;
    public event Action<int>? HealthChanged;
    
    private Inventory inventory;
    public Inventory Inventory => inventory;


    public Player(Vector2 position, Sprite sprite, GameContext gameContext)
    : base(position, sprite, 100, (float) gameContext.ScaleXY(3), gameContext)
    {

        hunger = new PlayerStat(maxStat);
        thirst = new PlayerStat(maxStat);
        oxygen = new PlayerStat(maxOxygen);
        this.inventory = new Inventory(); 
    }

    public async void Update(Vector2 mousePosition, InputManager input, GameTime gameTime)
    {

        updateHunger(gameTime);
        updateThirst(gameTime);
        updateOxygen(gameTime);

        float currentSpeed = this.speed;

        // Movement Keys
        if(input.IsKeyHeld(Keys.LeftShift))
        {
            currentSpeed *= 2;
            runningMultiplier = 0.5f;
        }
        else
        {
            runningMultiplier = 1f;
        }

        if (input.IsKeyHeld(Keys.A))
        {
            this.position.X -= currentSpeed;
        }

        if (input.IsKeyHeld(Keys.D))
        {
            this.position.X += currentSpeed;
        }

        if (input.IsKeyHeld(Keys.S))
        {
            this.position.Y += currentSpeed;
        }

        if (input.IsKeyHeld(Keys.W))
        {
            this.position.Y -= currentSpeed;
        }


        // Debug keys
        if (input.IsKeyPressed(Keys.NumPad1))
        {
            takeDamage(5);
        }
        if (input.IsKeyPressed(Keys.NumPad2))
        {
            takeDamage(10);
        }
        if (input.IsKeyPressed(Keys.NumPad3))
        {
            takeDamage(20);
        }

        if (input.IsKeyPressed(Keys.H))
        {
            gameContext.showHitboxes = !gameContext.showHitboxes;
        }



        if(damagedTimer > 0f)
        {
            damagedTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(damagedTimer <= 0f)
            {
                color = Color.White;
            }

        }

        Vector2 direction = mousePosition - position;
        float angle = System.MathF.Atan2(direction.Y, direction.X);
        sprite.Update(angle, position);
    }

    public void updateHunger(GameTime gameTime)
    {

        PlayerStat stat = hunger;

        stat.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (stat.timer >= 2.5f * runningMultiplier)
        {

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
                hunger = stat;
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

        if (stat.timer >= 1f * runningMultiplier)
        {
            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(3);
                oxygen = stat;
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

        if (stat.timer >= 2f * runningMultiplier)
        {
            
            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
                thirst = stat;
            }

            stat.timer = 0f;
            stat.value--;
            ThirstChanged?.Invoke(stat.value);

        }

        thirst = stat;

    }

    public override void takeDamage(int damage)
    {
        base.takeDamage(damage);

        color = damageColor;
        damagedTimer = 0.3f;

        SFX.Play(Assets.SoundEffects.playerDamage);
        HealthChanged?.Invoke(this.health);
    }

    public bool TryPickup(CatchableItem item)
    {
        if (item.Picked) return false;

        int quantityBefore = item.Stack.Quantity;
        int leftover = inventory.AddItem(item.Stack.Item, quantityBefore);
        int pickedAmount = quantityBefore - leftover;

        if (pickedAmount <= 0) return false;

        item.Stack.Remove(pickedAmount);

        if (item.Stack.Quantity == 0)
        {
            item.MarkPicked();
        }

        return true;
    }

    public bool TryConsume(Item item)
{
    if (item is not Consumable consumable) return false;

    PlayerStat stat = thirst;
    stat.value = Math.Min(stat.value + consumable.ThirstRestore, stat.max);
    thirst = stat;
    ThirstChanged?.Invoke(stat.value);

    inventory.RemoveItem(item, 1);
    return true;
}



}