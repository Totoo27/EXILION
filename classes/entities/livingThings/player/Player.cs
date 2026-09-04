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

        if(damagedTimer > 0f)
        {
            damagedTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(damagedTimer <= 0f)
            {
                color = Color.White;
            }

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

        if (stat.timer >= 2.5f)
        {

            takeDamage(1);

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

        if (stat.timer >= 1f)
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

        if (stat.timer >= 2f)
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



}