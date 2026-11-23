using System;
using Microsoft.Xna.Framework;
using EXILION.Items;
using EXILION.World;
using EXILION.Logic;
using System.Collections.Generic;

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

    public event Action<Item?>? UpdateHandsOffset;
    public event Action<float>? ToolAttack;
    public event Action<GameAudioCue>? SoundRequested;
    private Item? itemInHand;
    public Vector2 ItemPosition { get; private set; }
    public float ItemAngle { get; private set; }

    // Hand distance Offset
    private const float DefaultSideDistance = 30f;
    private const float DefaultForwardDistance = 20f;
    private const float TwoHandedSideDistance = 16f;
    private const float TwoHandedForwardDistance = 26f;
    private float sideDistance = DefaultSideDistance;
    private float forwardDistance = DefaultForwardDistance;

    // Carrying item offset
    private const float TwoHandedItemSideOffset = -15f;
    private const float TwoHandedItemForwardOffset = 8f;
    private const float ToolItemSideOffset = -6f;
    private const float ToolItemForwardOffset = 15f;
    private float itemSideOffset = 0;
    private float itemForwardOffset = 0;
    private bool isSupportingToolAttack;
    private const float ConsumeDuration = 0.5f;
    private const float ConsumeReach = 8f;
    private float consumeTimer;
    private bool isConsuming;
    private Item? consumingItem;
    private Player? consumingPlayer;

    // Attack properties
    private const float DefaultAttackDistance = 35f;
    private const float HandWithToolAttackDistance = 28f;
    private const float ToolForwardAttackDistance = 13f;
    private const float ToolSideAttackDistance = -15f;
    private const float DefaultAttackDuration = 0.14f;
    private float CurrentAttackDuration = DefaultAttackDuration;
    public const float DefaultDamage = 3f;
    public AttackHitbox? CurrentHitbox { get; private set; }

    private const float ToolAttackRotation = MathHelper.PiOver2; // 90 grados

    private int HitboxSize;

    private HandState state = HandState.Idle;
    private readonly HashSet<WorldObjectId> hitObjectsThisAttack = new();
    private readonly HashSet<Creature> hitCreaturesThisAttack = new();

    private float attackTimer = 0f;

    private Vector2 ownerPosition;
    private Vector2 direction = Vector2.UnitX;

    public Vector2 Position { get; private set; }
    public float Angle { get; private set; }

    public bool IsAttacking => state != HandState.Idle;
    public bool IsRetracting => state == HandState.Retracting;
    public bool CanAttack => state == HandState.Idle && (itemInHand is Tool || itemInHand?.TwoHanded != true);
    public bool hasItem => itemInHand != null;
    public bool hasConsumable => itemInHand != null && itemInHand is Consumable;
    public Item? HeldItem => itemInHand;
    public int AttackDamage => itemInHand is Tool tool ? Math.Max(1, (int)MathF.Ceiling(tool.Damage)) : (int)DefaultDamage;

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

    public Hand(int hitboxSize)
    {
        HitboxSize = hitboxSize;
    }

    public void Update(Vector2 ownerPosition, Vector2 direction, bool isLeft, float deltaTime)
    {
        this.ownerPosition = ownerPosition;

        if (direction != Vector2.Zero)
        {
            this.direction = Vector2.Normalize(direction);
        }

        Angle = MathF.Atan2(this.direction.Y, this.direction.X) - MathF.PI/2f;
        UpdateAttack(deltaTime);
        UpdateConsume(deltaTime);
        UpdatePosition(isLeft);
        UpdateHitbox(ItemPosition, ItemAngle);

        if (isConsuming)
        {
            float progress = MathHelper.Clamp(consumeTimer / ConsumeDuration, 0f, 1f);
            Vector2 towardPlayer = ownerPosition - ItemPosition;
            if (towardPlayer != Vector2.Zero)
            {
                towardPlayer.Normalize();
                ItemPosition += towardPlayer * ConsumeReach * MathF.Sin(progress * MathF.PI);
            }
        }

    }

    public bool DoAction(Player player)
    {
        if(itemInHand is Consumable)
        {
            if (!isConsuming)
            {
                isConsuming = true;
                consumeTimer = 0f;
                consumingItem = itemInHand;
                consumingPlayer = player;
            }
            return true;
        }

        if(itemInHand is Tool)
        {

            if (!Attack()) return false;

            ToolAttack?.Invoke(CurrentAttackDuration);
            return true;

        }

        if(itemInHand?.TwoHanded == true) return true; // Avoid attacking with two-handed on the left hand

        return false;
    }

    private void UpdateConsume(float deltaTime)
    {
        if (!isConsuming) return;

        consumeTimer += deltaTime;
        if (consumeTimer < ConsumeDuration) return;

        Item? item = consumingItem;
        Player? player = consumingPlayer;
        isConsuming = false;
        consumingItem = null;
        consumingPlayer = null;
        consumeTimer = 0f;

        if (item != null && player != null)
        {
            player.TryConsume(item);
        }
    }

    public void StartToolAttack(float attackDuration)
    {

        if (state != HandState.Idle) return;

        isSupportingToolAttack = true;
        CurrentAttackDuration = attackDuration;

        state = HandState.Extending;
        attackTimer = 0f;


    }

    public bool Attack()
    {
        if (!CanAttack) return false;

        hitObjectsThisAttack.Clear();
        hitCreaturesThisAttack.Clear();
        state = HandState.Extending;
        attackTimer = 0f;
        SoundRequested?.Invoke(GameAudioCue.HandSwing);

        return true;
    }

    public void CancelAttack()
    {
        if (isSupportingToolAttack)
        {
            CurrentAttackDuration = DefaultAttackDuration;
        }

        state = HandState.Idle;
        attackTimer = 0f;
        isSupportingToolAttack = false;
        CurrentHitbox = null;
    }

    public bool RegisterHit(WorldObjectId objectId) => hitObjectsThisAttack.Add(objectId);
    public bool RegisterHit(Creature creature) => hitCreaturesThisAttack.Add(creature);

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
                CurrentAttackDuration = isSupportingToolAttack ? DefaultAttackDuration : CurrentAttackDuration;
                isSupportingToolAttack = false;
            }
        }
    }

    private void UpdateHitbox(Vector2 ItemPosition, float ItemAngle)
    {
        if (state == HandState.Idle || state == HandState.Retracting || isSupportingToolAttack)
        {
            CurrentHitbox = null;
            return;
        }

        if(itemInHand is Tool tool)
        {
            CurrentHitbox = new AttackHitbox(
                ItemPosition,
                tool.HitboxWidth,
                tool.HitboxHeight,
                ItemAngle
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
        float attackDistance = itemInHand is Tool || isSupportingToolAttack ? (isLeft ? -HandWithToolAttackDistance/2 : HandWithToolAttackDistance) : DefaultAttackDistance;

        Vector2 basePosition = ownerPosition + direction * forwardDistance + perpendicular * sideOffset;

        // Movimiento normal de la mano
        Vector2 attackOffset = Vector2.Zero;

        if (state == HandState.Extending)
        {
            float progress = attackTimer / CurrentAttackDuration;
            attackOffset = direction * attackDistance * EaseOut(progress);
        }
        else if (state == HandState.Retracting)
        {
            float progress = attackTimer / CurrentAttackDuration;
            attackOffset = direction * attackDistance * (1f - EaseOut(progress));
        }


        Position = basePosition + attackOffset;

        // Item Position Offset

        ItemPosition = Position + direction * itemForwardOffset + perpendicular * itemSideOffset;

        if (itemInHand is Tool)
        {
            Vector2 toolAttackOffset = Vector2.Zero;

            if (state == HandState.Extending)
            {
                float progress = attackTimer / CurrentAttackDuration;
                toolAttackOffset = direction * ToolForwardAttackDistance * EaseOut(progress) + perpendicular * ToolSideAttackDistance * EaseOut(progress);
            }
            else if (state == HandState.Retracting)
            {
                float progress = attackTimer / CurrentAttackDuration;
                toolAttackOffset = direction * ToolForwardAttackDistance * (1f - EaseOut(progress)) + perpendicular * ToolSideAttackDistance * (1f - EaseOut(progress));
            }

            ItemPosition += toolAttackOffset - attackOffset;
        }

        ItemAngle = Angle;

        if (itemInHand is Tool)
        {
            ItemAngle += 3 * MathF.PI / 2f;

            if (state == HandState.Extending)
            {
                float progress = attackTimer / CurrentAttackDuration;
                ItemAngle -= ToolAttackRotation * EaseOut(progress);
            }
            else if (state == HandState.Retracting)
            {
                float progress = attackTimer / CurrentAttackDuration;
                ItemAngle -= ToolAttackRotation * (1f - EaseOut(progress));
            }
        }
    }

    private float EaseOut(float value)
    {
        value = MathHelper.Clamp(value, 0f, 1f);
        return 1f - MathF.Pow(1f - value, 3f);
    }

    public void updateItemInHand(Item? item)
    {
        if(item == itemInHand) return;

        UpdateHandsOffset?.Invoke(item);

        CurrentAttackDuration = item is Tool tool ? 1f / tool.AttackSpeed : DefaultAttackDuration;

        itemInHand = item;
    }

    public void ManageOffsets(Item? item)
    {

        if(item == null || !item.TwoHanded)
        {
            sideDistance = DefaultSideDistance;
            forwardDistance = DefaultForwardDistance;
            itemSideOffset = 0;
            itemForwardOffset = 0;  
            return; 
        }

        if(item.TwoHanded)
        {
            sideDistance = TwoHandedSideDistance;
            forwardDistance = TwoHandedForwardDistance;
            itemSideOffset = TwoHandedItemSideOffset;
            itemForwardOffset = TwoHandedItemForwardOffset;
        }

        if(item is Tool)
        {
            itemSideOffset = ToolItemSideOffset;
            itemForwardOffset = ToolItemForwardOffset;
            return;
        }

    }

}
