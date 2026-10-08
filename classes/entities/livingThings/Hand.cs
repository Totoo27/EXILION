using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;

namespace EXILION.Entities.LivingThings;


public class Hand
{
    #nullable enable
    private enum HandState
    {
        Idle,
        Extending,
        Retracting
    }

    public event Action<bool>? ConsumableOnHand;
    private Item? itemInHand;
    private Sprite? itemSprite;

    // Hand distance Offset
    private const float DefaultSideDistance = 32f;
    private const float DefaultForwardDistance = 20f;
    private const float EatingSideDistance = 15f;
    private const float EatingForwardDistance = 26f;
    private float sideDistance = DefaultSideDistance;
    private float forwardDistance = DefaultForwardDistance;

    // Carrying item offset
    private const float ConsumableItemSideOffset = -15f;
    private const float ConsumableItemForwardOffset = 8f;
    private float itemSideOffset = 0;
    private float itemForwardOffset = 0;

    // Attack properties
    private const float DefaultAttackDistance = 35f;
    private const float DefaultAttackDuration = 0.14f;
    private float CurrentAttackDuration => itemInHand is Tool tool ? 1f / tool.AttackSpeed : DefaultAttackDuration;
    public const float DefaultDamage = 5f;
    public AttackHitbox? CurrentHitbox { get; private set; }

    private int HitboxSize;

    private HandState state = HandState.Idle;

    private float attackTimer = 0f;

    private Vector2 ownerPosition;
    private Vector2 direction = Vector2.UnitX;

    public Vector2 Position { get; private set; }
    private Sprite sprite;

    public float Angle { get; private set; }

    public bool IsAttacking => state != HandState.Idle;
    public bool CanAttack => state == HandState.Idle && itemInHand is not Consumable;
    public bool hasItem => itemInHand != null;
    public bool hasConsumable => itemInHand != null && itemInHand is Consumable;

    public Rectangle Hitbox
    {
        get
        {
            return new Rectangle(
                (int)(Position.X - HitboxSize / 2f),
                (int)(Position.Y - HitboxSize / 2f),
                HitboxSize,
                HitboxSize
            );
        }
    }

    public Hand(Texture2D texture, int hitboxSize)
    {
        sprite = new Sprite(texture, 0.8f);
        HitboxSize = hitboxSize;
    }

    public void Update(Vector2 ownerPosition, Vector2 direction, bool isLeft, GameTime gameTime)
    {
        this.ownerPosition = ownerPosition;

        if (direction != Vector2.Zero)
        {
            this.direction = Vector2.Normalize(direction);
        }

        Angle = MathF.Atan2(this.direction.Y, this.direction.X) - MathF.PI/2f;
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateAttack(deltaTime);
        UpdatePosition(isLeft);
        UpdateHitbox();

        sprite.Update(Angle, Position);

        Vector2 perpendicular = new Vector2(-this.direction.Y, this.direction.X);

        Vector2 itemPosition = Position + this.direction * itemForwardOffset + perpendicular * itemSideOffset;

        itemSprite?.Update(Angle, itemPosition);
    }

    public bool DoAction(Player player)
    {
        if(itemInHand is Consumable)
        {
            player.TryConsume(itemInHand);
            return true;
        }

        if(itemInHand is Tool)
        {
            return Attack();
        }

        return false;
    }

    public bool Attack()
    {
        if (!CanAttack) return false;

        state = HandState.Extending;
        attackTimer = 0f;
        SFX.Play(Assets.SoundEffects.swings[Random.Shared.Next(Assets.SoundEffects.swings.Length)]);

        return true;
    }

    private void UpdateAttack(float deltaTime)
    {
        if (state == HandState.Idle) return;

        attackTimer += deltaTime;

        if (attackTimer >= CurrentAttackDuration)
        {
            attackTimer = 0f;

            if (state == HandState.Extending)
            {
                state = HandState.Retracting;
            }

            else if (state == HandState.Retracting)
            {
                state = HandState.Idle;
            }
        }
    }

    private void UpdateHitbox()
    {
        if (state == HandState.Idle)
        {
            CurrentHitbox = null;
            return;
        }

        if(itemInHand is Tool tool)
        {
            CurrentHitbox = new AttackHitbox(
                Position,
                tool.HitboxWidth,
                tool.HitboxHeight,
                Angle
            );
        } else
        {
            CurrentHitbox = new AttackHitbox(
                Position,
                20f,
                20f,
                Angle
            );
        }
        

    }

    private void UpdatePosition(bool isLeft)
    {
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        float sideOffset = isLeft ? -sideDistance : sideDistance;

        Vector2 basePosition = ownerPosition + direction * forwardDistance + perpendicular * sideOffset;
        Vector2 attackOffset = Vector2.Zero;

        if (state == HandState.Extending)
        {
            float progress = attackTimer / CurrentAttackDuration;
            float easedProgress = EaseOut(progress);
            attackOffset = direction * DefaultAttackDistance * easedProgress;
        }
        else if (state == HandState.Retracting)
        {
            float progress = attackTimer / CurrentAttackDuration;
            float easedProgress = EaseOut(progress);
            attackOffset = direction * DefaultAttackDistance * (1f - easedProgress);
        }

        Position = basePosition + attackOffset;
    }

    private float EaseOut(float value)
    {
        value = MathHelper.Clamp(value, 0f, 1f);
        return 1f - MathF.Pow(1f - value, 3f);
    }

    public void updateItemInHand(Item? item)
    {
        if(item == itemInHand) return;

        ConsumableOnHand?.Invoke(item is Consumable);

        float ItemScale = item is Tool ? 1.5f : 0.8f;

        itemInHand = item;
        itemSprite = item != null ? new Sprite(item.Icon, ItemScale) : null;
    }

    public void ToggleEating(bool eating)
    {
        if(eating)
        {
            sideDistance = EatingSideDistance;
            forwardDistance = EatingForwardDistance;
            itemSideOffset = ConsumableItemSideOffset;
            itemForwardOffset = ConsumableItemForwardOffset;
            return;
        }

        sideDistance = DefaultSideDistance;
        forwardDistance = DefaultForwardDistance;
        itemSideOffset = 0;
        itemForwardOffset = 0;
    }

    public void Draw(SpriteBatch spriteBatch, Color color)
    {
        sprite.Draw(spriteBatch, color);
        itemSprite?.Draw(spriteBatch, color);

        if(CurrentHitbox != null)
        {
            Texture2D hitboxTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            hitboxTexture.SetData(new[] { Color.White });

            spriteBatch.Draw(hitboxTexture, CurrentHitbox?.Position ?? Vector2.Zero, null, Color.Red * 0.5f, CurrentHitbox?.Rotation ?? 0f, new Vector2(0.5f, 0.5f), new Vector2(CurrentHitbox?.Width ?? 0f, CurrentHitbox?.Height ?? 0f), SpriteEffects.None, 0f);
        }
    }

}