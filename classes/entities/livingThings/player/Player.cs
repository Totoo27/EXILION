using System;
using Microsoft.Xna.Framework;
using EXILION.Items;
using EXILION.Entities.CatchableItems;
using EXILION.Saving;
using EXILION.World;
using EXILION.Logic;

namespace EXILION.Entities.LivingThings;

public class Player : LivingThing
{
    #nullable enable
    public const int maxStat = 100;
    private int maxOxygen = 100;
    private float runningMultiplier = 1f;
    private const float ItemTransitionDuration = 0.5f;
    private float itemTransitionTimer;
    private Item? pendingHandItem;
    private bool hasPendingHandItem;
    private const float AttackHeadTurn = 0.16f;
    private const float AttackHeadTurnWeapon = 0.25f;

    private float headTurn => rightHand.HeldItem is Tool ? AttackHeadTurnWeapon : AttackHeadTurn;
    private float headAttackRotation;
    public float VisualAngle { get; private set; }

    public PlayerStat oxygen {get; private set;}
    public PlayerStat hunger {get; private set;}
    public PlayerStat thirst {get; private set;}

    public event Action<int>? OxygenChanged;
    public event Action<int>? HungerChanged;
    public event Action<int>? ThirstChanged;
    public event Action<int>? HealthChanged;
    public event Action<int>? DamageTaken;
    
    private Inventory inventory;
    public Inventory Inventory => inventory;

    private const int handHitboxSize = 20;
    public Hand rightHand { get; private set; } = new Hand(handHitboxSize);
    public Hand leftHand { get; private set; } = new Hand(handHitboxSize);
    private bool IsLeftNext = false;

    private float combatTimer = 0f;
    private const float combatCooldown = 3f; // Time in seconds before health regeneration can start after taking damage
    private float healthRegenerationAccumulator;
    private bool canRegenerate => health < maxHealth && combatTimer <= 0f && hunger.value > 0 && thirst.value > 0 && oxygen.value > 0;


    public Player(Vector2 position, Point hitboxSize)
    : base(position, 100, PlayerMovementSettings.GroundSpeed, GameAudioCue.PlayerDamage,
        hitboxSize)
    {

        hunger = new PlayerStat(maxStat);
        thirst = new PlayerStat(maxStat);
        oxygen = new PlayerStat(maxOxygen);
        this.inventory = new Inventory(); 

        rightHand.UpdateHandsOffset += rightHand.ManageOffsets;
        rightHand.UpdateHandsOffset += leftHand.ManageOffsets;
        rightHand.ToolAttack += leftHand.StartToolAttack;
        rightHand.SoundRequested += RequestSound;
        leftHand.SoundRequested += cue =>
        {
            if (rightHand.HeldItem is not Tool)
            {
                RequestSound(cue);
            }
        };
    }

    public void Restore(PlayerSaveData saveData)
    {
        this.position = new Vector2(saveData.PositionX, saveData.PositionY);
        this.health = saveData.Health;
        this.hunger = saveData.Hunger;
        this.thirst = saveData.Thirst;
        this.oxygen = saveData.Oxygen;

        inventory.Restore(saveData.Inventory);
        inventory.RestoreArmor(saveData.EquippedArmorItemId);
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

            Inventory = inventory.GetSaveData(),
            EquippedArmorItemId = inventory.EquippedArmor?.Id
        };

    }

    public void Update(Vector2 mousePosition, PlayerInput input, float deltaTime, World.World world)
    {
        base.Update(deltaTime);

        updateHunger(deltaTime);
        updateThirst(deltaTime);
        updateOxygen(deltaTime, world != null && world.IsOxygenSupplied(position));
        combatTimer = MathF.Max(0f, combatTimer - deltaTime);
        itemTransitionTimer = MathF.Max(0f, itemTransitionTimer - deltaTime);

        if (hasPendingHandItem && itemTransitionTimer <= 0f)
        {
            rightHand.updateItemInHand(pendingHandItem);
            pendingHandItem = null;
            hasPendingHandItem = false;
        }

        if (canRegenerate)
        {
            float regenRate = 2f; // Health points per second
            healthRegenerationAccumulator += regenRate * deltaTime;
            int healthToRestore = (int)healthRegenerationAccumulator;
            if (healthToRestore > 0)
            {
                int previousHealth = health;
                health = Math.Min(maxHealth, health + healthToRestore);
                healthRegenerationAccumulator -= health - previousHealth;
                if (health != previousHealth) HealthChanged?.Invoke(health);
                if (health >= maxHealth) healthRegenerationAccumulator = 0f;
            }
        }
        else if (health >= maxHealth)
        {
            healthRegenerationAccumulator = 0f;
        }

        // Movement

        runningMultiplier = 1f;

        Vector2 delta = Vector2.Zero;

        float armorSpeedMultiplier = inventory.EquippedArmor?.SpeedMultiplier ?? 1f;
        float currentSpeed = speed * armorSpeedMultiplier * (input.Run ? 1.5f : 1f);

        if (input.MoveLeft) delta.X -= 1f;
        if (input.MoveRight) delta.X += 1f;
        if (input.MoveDown) delta.Y += 1f;
        if (input.MoveUp) delta.Y -= 1f;
        if (delta != Vector2.Zero)
        {
            delta.Normalize();
        }

        delta *= currentSpeed * deltaTime;

        if (delta != Vector2.Zero && input.Run)
        {
            runningMultiplier = 0.5f;
        }

        Move(delta, deltaTime, world, currentSpeed / speed);

        // Hands

        if (itemTransitionTimer <= 0f && input.PrimaryActionPressed && !input.PrimaryActionCaptured)
        {
            if (rightHand.HeldItem?.Id == ItemRegistry.BotellaVacia.Id && world != null
                && TryFillBottle(world)) return;

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

        Vector2 direction = mousePosition - position;
        float angle = MathF.Atan2(direction.Y, direction.X);

        rightHand.Update(position, direction, false, deltaTime);
        leftHand.Update(position, direction, true, deltaTime);

        float targetHeadRotation = 0f;
        if (rightHand.HeldItem is Tool &&
            ((rightHand.IsAttacking && !rightHand.IsRetracting) ||
             (leftHand.IsAttacking && !leftHand.IsRetracting)))
        {
            // Las herramientas atacan desde la mano derecha.
            targetHeadRotation = -headTurn;
        }
        else if (rightHand.IsAttacking && !rightHand.IsRetracting)
        {
            targetHeadRotation = -headTurn;
        }
        else if (leftHand.IsAttacking && !leftHand.IsRetracting)
        {
            targetHeadRotation = headTurn;
        }

        headAttackRotation = MathHelper.Lerp(
            headAttackRotation,
            targetHeadRotation,
            MathHelper.Clamp(deltaTime * 12f, 0f, 1f)
        );
        VisualAngle = angle + headAttackRotation;
    }

    public void BeginItemTransition(Item? item)
    {
        pendingHandItem = item;
        hasPendingHandItem = true;
        itemTransitionTimer = ItemTransitionDuration;
        rightHand.CancelAttack();
        leftHand.CancelAttack();
    }

    public void SetHandItemImmediately(Item? item)
    {
        itemTransitionTimer = 0f;
        pendingHandItem = null;
        hasPendingHandItem = false;
        rightHand.updateItemInHand(item);
    }

    public void updateHunger(float deltaTime)
    {

        PlayerStat stat = hunger;

        stat.timer += deltaTime;

        if (stat.timer >= 2.5f * runningMultiplier)
        {

            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeNaturalDamage(1);
            }

            HungerChanged?.Invoke(stat.value);

        }

        hunger = stat;

    }

    public void updateOxygen(float deltaTime, bool oxygenSupplied = false)
    {
        PlayerStat stat = oxygen;

        if (oxygenSupplied)
        {
            stat.timer += deltaTime;
            bool changed = false;
            const float oxygenRecoveryInterval = 0.2f;
            while (stat.timer >= oxygenRecoveryInterval && stat.value < stat.max)
            {
                stat.timer -= oxygenRecoveryInterval;
                stat.value++;
                changed = true;
            }
            if (stat.value >= stat.max)
                stat.timer = 0f;
            oxygen = stat;
            if (changed) OxygenChanged?.Invoke(stat.value);
            return;
        }

        stat.timer += deltaTime;

        if (stat.timer >= 1f * runningMultiplier)
        {
            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeNaturalDamage(3);
            }

            OxygenChanged?.Invoke(stat.value);

        }

        oxygen = stat;

    }

    public void updateThirst(float deltaTime)
    {

        PlayerStat stat = thirst;

        stat.timer += deltaTime;

        if (stat.timer >= 2f * runningMultiplier)
        {

            stat.timer = 0f;
            stat.value--;

            if (stat.value <= 0)
            {
                stat.value = 0;
                takeNaturalDamage(1);
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
        if (isDead || damage <= 0) return;

        float protection = (inventory.EquippedArmor?.ArmorPoints ?? 0f) / 100f;
        damage = Math.Max(1, (int)MathF.Ceiling(damage * (1f - protection)));

        int previousHealth = health;
        base.takeDamage(damage);
        int damageReceived = previousHealth - health;
        if (damageReceived <= 0) return;

        combatTimer = combatCooldown;
        healthRegenerationAccumulator = 0f;
        HealthChanged?.Invoke(health);
        DamageTaken?.Invoke(damageReceived);
    }

    private void takeNaturalDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        int previousHealth = health;
        base.takeDamage(damage);
        int damageReceived = previousHealth - health;
        if (damageReceived <= 0) return;

        combatTimer = combatCooldown;
        healthRegenerationAccumulator = 0f;
        HealthChanged?.Invoke(health);
        DamageTaken?.Invoke(damageReceived);
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

        RequestSound(GameAudioCue.Pickup);
        return true;
    }

    public bool TryConsume(Item item)
    {
        if (item is not Consumable consumable) return false;

        PlayerStat stat;
        GameAudioCue soundCue;

        switch (consumable.statType)
        {
            case StatType.Thirst:
                stat = thirst;
                stat.value = Math.Min(stat.value + consumable.statRestore, stat.max);
                thirst = stat;
                ThirstChanged?.Invoke(thirst.value);

                soundCue = GameAudioCue.Drink;
            break;

            case StatType.Hunger:
                stat = hunger;
                stat.value = Math.Min(stat.value + consumable.statRestore, stat.max);
                hunger = stat;
                HungerChanged?.Invoke(hunger.value);

                soundCue = GameAudioCue.Eat;
            break;

            case StatType.Oxygen:
                stat = oxygen;
                stat.value = Math.Min(stat.value + consumable.statRestore, stat.max);
                oxygen = stat;
                OxygenChanged?.Invoke(oxygen.value);

                soundCue = GameAudioCue.BottleBreath;
            break;

            default:
                soundCue = GameAudioCue.PlayerDamage;
            break;
        }

        RequestSound(soundCue);
        inventory.RemoveItem(item, 1);
        if (consumable.statType == StatType.Thirst || consumable.statType == StatType.Oxygen)
            inventory.AddItem(ItemRegistry.BotellaVacia, 1);
        
        return true;
    }

    private bool TryFillBottle(World.World world)
    {
        Item? filledBottle = world.IsInWater(position)
            ? ItemRegistry.AguaPurificada
            : world.IsOxygenSupplied(position) ? ItemRegistry.OxigenoEmbotellado : null;
        if (filledBottle == null || !inventory.HasItem(ItemRegistry.BotellaVacia, 1)) return false;

        inventory.RemoveItem(ItemRegistry.BotellaVacia, 1);
        inventory.AddItem(filledBottle, 1);
        return true;
    }

    public CraftResult EvaluateCraft(Recipe recipe, bool atCraftingTable = false)
    {
        if (recipe == null) return CraftResult.MissingIngredients;

        if (recipe.RequiresCraftingTable && !atCraftingTable)
        {
            return CraftResult.MissingCraftingTable;
        }

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

    public bool CanCraft(Recipe recipe, bool atCraftingTable = false)
    {
        return EvaluateCraft(recipe, atCraftingTable) == CraftResult.Success;
    }

    public CraftResult TryCraft(Recipe recipe, bool atCraftingTable = false)
    {
        CraftResult result = EvaluateCraft(recipe, atCraftingTable);
        if (result != CraftResult.Success) return result;

        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            inventory.RemoveItem(ingredient.Item, ingredient.Quantity);
        }

        inventory.AddItem(recipe.Result, recipe.Amount);
        return CraftResult.Success;
    }

}

