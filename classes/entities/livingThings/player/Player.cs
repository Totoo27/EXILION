using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Items;
using EXILION.Entities.CatchableItems;
using EXILION.UI;
using Microsoft.Xna.Framework.Audio;

namespace EXILION.Entities.LivingThings;

public class Player : LivingThing
{
    #nullable enable
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

    private const int handHitboxSize = 20;
    public Hand rightHand { get; private set; } = new Hand(Assets.Sprites.playerHand, handHitboxSize);
    public Hand leftHand { get; private set; } = new Hand(Assets.Sprites.playerHand, handHitboxSize);
    private bool IsLeftNext = false;


    public Player(Vector2 position, Sprite sprite, GameContext gameContext)
    : base(position, sprite, 100, 200, gameContext)
    {

        hunger = new PlayerStat(maxStat);
        thirst = new PlayerStat(maxStat);
        oxygen = new PlayerStat(maxOxygen);
        this.inventory = new Inventory(); 

        rightHand.UpdateHandsOffset += rightHand.ManageOffsets;
        rightHand.UpdateHandsOffset += leftHand.ManageOffsets;
        rightHand.ToolAttack += leftHand.StartToolAttack;
    }

    public void Update(Vector2 mousePosition, InputManager input, GameTime gameTime)
    {

        updateHunger(gameTime);
        updateThirst(gameTime);
        updateOxygen(gameTime);

        float currentSpeed = this.speed;
        runningMultiplier = 1f;

        Vector2 movementDirection = Vector2.Zero;

        // Movement Keys
        if (input.IsKeyHeld(Keys.A)) movementDirection.X -= 1;

        if (input.IsKeyHeld(Keys.D)) movementDirection.X += 1;

        if (input.IsKeyHeld(Keys.W)) movementDirection.Y -= 1;

        if (input.IsKeyHeld(Keys.S)) movementDirection.Y += 1;


        if (movementDirection != Vector2.Zero)
        {
            // Sprint
            if(input.IsKeyHeld(Keys.LeftShift))
            {
                currentSpeed *= 2;
                runningMultiplier = 0.5f;
            }

            movementDirection.Normalize();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += movementDirection * currentSpeed * deltaTime;

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

        if (input.IsLeftMousePressed())
        {

            if (rightHand.hasItem)
            {
                if(rightHand.DoAction(this)) return;
            }

            // Hit with raw hand
            if(rightHand.CanAttack && !leftHand.IsAttacking && !IsLeftNext)
            {
                rightHand.Attack();
                IsLeftNext = true;
            } 
            else if(leftHand.CanAttack && !rightHand.IsAttacking && IsLeftNext)
            {
                leftHand.Attack();
                IsLeftNext = false;
            }
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
        rightHand.Update(position, direction, false, gameTime);
        leftHand.Update(position, direction, true, gameTime);
        sprite.Update(angle, position);
    }

    public void updateHunger(GameTime gameTime)
    {

        PlayerStat stat = hunger;

        stat.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (stat.timer >= 2.5f * runningMultiplier)
        {

            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
            }

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
            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(3);
            }

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

            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeDamage(1);
            }

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

        SFX.Play(Assets.SoundEffects.pickUpItem);
        return true;
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {

        leftHand.Draw(spriteBatch, color, gameContext.showHitboxes);
        rightHand.Draw(spriteBatch, color, gameContext.showHitboxes);

        sprite.Draw(spriteBatch, color);

        if (gameContext.showHitboxes)
        {
            spriteBatch.Draw(pixel, hitbox, Color.Red);
        }
        
    }

    public bool TryConsume(Item item)
    {
        if (item is not Consumable consumable) return false;

        PlayerStat stat;
        SoundEffect sfx;

        switch (consumable.statType)
        {
            case StatType.Thirst:
                stat = thirst;
                stat.value = Math.Min(stat.value + consumable.statRestore, stat.max);
                thirst = stat;
                ThirstChanged?.Invoke(thirst.value);

                sfx = Assets.SoundEffects.drink;
            break;

            case StatType.Hunger:
                stat = hunger;
                stat.value = Math.Min(stat.value + consumable.statRestore, stat.max);
                hunger = stat;
                HungerChanged?.Invoke(hunger.value);

                sfx = Assets.SoundEffects.eat;
            break;

            case StatType.Oxygen:
                stat = oxygen;
                stat.value = Math.Min(stat.value + consumable.statRestore, stat.max);
                oxygen = stat;
                OxygenChanged?.Invoke(oxygen.value);

                sfx = Assets.SoundEffects.bottleBreath;
            break;

            default:
                sfx = Assets.SoundEffects.playerDamage;
            break;
        }

        SFX.Play(sfx);
        inventory.RemoveItem(item, 1);
        
        return true;
    }

}