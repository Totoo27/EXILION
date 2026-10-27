using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EXILION.Items;
using EXILION.Entities.CatchableItems;
using EXILION.Saving;
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

    public void Restore(PlayerSaveData saveData)
    {
        this.position = new Vector2(saveData.PositionX, saveData.PositionY);
        this.health = saveData.Health;
        this.hunger = saveData.Hunger;
        this.thirst = saveData.Thirst;
        this.oxygen = saveData.Oxygen;

        inventory.Restore(saveData.Inventory);
    }

    public PlayerSaveData GetSaveData()
    {
        
        return new PlayerSaveData
        {
            PositionX = this.position.X,
            PositionY = this.position.Y,

            Health = this.health,
            Hunger = this.hunger,
            Thirst = this.thirst,
            Oxygen = this.oxygen,

            Inventory = inventory.GetSaveData()
        };

    }

    public void Update(Vector2 mousePosition, InputManager input, GameTime gameTime, World.World world)
    {
        updateHunger(gameTime);
        updateThirst(gameTime);
        updateOxygen(gameTime);

        // Movement

        float currentSpeed = this.speed;
        float collisionRadius = hitbox.Width / 2f;
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        runningMultiplier = 1f;

        Vector2 delta = Vector2.Zero;

        if (input.IsKeyHeld(Keys.LeftShift))
        {
            currentSpeed *= 1.5f;
        }

        if (input.IsKeyHeld(Keys.A)) delta.X -= 1f;
        if (input.IsKeyHeld(Keys.D)) delta.X += 1f;
        if (input.IsKeyHeld(Keys.S)) delta.Y += 1f;
        if (input.IsKeyHeld(Keys.W)) delta.Y -= 1f;
        if (delta != Vector2.Zero)
        {
            delta.Normalize();
        }

        delta *= currentSpeed * deltaTime;

        if (delta != Vector2.Zero && currentSpeed == speed * 1.5f)
        {
            runningMultiplier = 0.5f;
        }

        Vector2 nextX = new Vector2(
            position.X + delta.X,
            position.Y
        );

        if (world == null || world.IsWalkable(nextX, collisionRadius))
        {
            position.X = nextX.X;
        }

        Vector2 nextY = new Vector2(
            position.X,
            position.Y + delta.Y
        );

        if (world == null || world.IsWalkable(nextY, collisionRadius))
        {
            position.Y = nextY.Y;
        }

        // Debug

        if (input.IsKeyPressed(Keys.H))
        {
            gameContext.showHitboxes = !gameContext.showHitboxes;
        }

        // Hands

        if (input.IsLeftMousePressed() && !input.IsMouseCaptured)
        {
            if (rightHand.hasItem)
            {
                if (rightHand.DoAction(this)) return;
            }

            // Hit with raw hand
            if (rightHand.CanAttack && !leftHand.IsAttacking && !IsLeftNext)
            {
                rightHand.Attack();
                IsLeftNext = true;
            }
            else if (leftHand.CanAttack && !rightHand.IsAttacking && IsLeftNext)
            {
                leftHand.Attack();
                IsLeftNext = false;
            }
        }

        // Damage timer

        if (damagedTimer > 0f)
        {
            damagedTimer -= deltaTime;

            if (damagedTimer <= 0f)
            {
                color = Color.White;
            }
        }

        Vector2 direction = mousePosition - position;
        float angle = MathF.Atan2(direction.Y, direction.X);

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

    public void ForceUpdateStats()
    {   
        HealthChanged?.Invoke(health);
        OxygenChanged?.Invoke(oxygen.value);
        HungerChanged?.Invoke(hunger.value);
        ThirstChanged?.Invoke(thirst.value);
    }

    public override void takeDamage(int damage)
    {
        base.takeDamage(damage);

        color = damageColor;
        damagedTimer = 0.3f;

        SFX.Play(Assets.SoundEffects.playerDamage);
        HealthChanged?.Invoke(this.health);
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

    // Item usage
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

    public CraftResult EvaluateCraft(Recipe recipe)
{
    if (recipe == null) return CraftResult.MissingIngredients;

    foreach (Ingredient ingredient in recipe.Ingredients)
    {
        if (!inventory.HasItem(ingredient.Item, ingredient.Quantity))
        {
            return CraftResult.MissingIngredients;
        }
    }

    if (!inventory.CanFit(recipe.Result, recipe.Amount))
    {
        return CraftResult.InventoryFull;
    }

    return CraftResult.Success;
}

public bool CanCraft(Recipe recipe)
{
    return EvaluateCraft(recipe) == CraftResult.Success;
}

public CraftResult TryCraft(Recipe recipe)
{
    CraftResult result = EvaluateCraft(recipe);
    if (result != CraftResult.Success) return result;

    foreach (Ingredient ingredient in recipe.Ingredients)
    {
        inventory.RemoveItem(ingredient.Item, ingredient.Quantity);
    }

    inventory.AddItem(recipe.Result, recipe.Amount);
    return CraftResult.Success;
}

}