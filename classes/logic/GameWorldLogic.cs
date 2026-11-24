using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using EXILION.Entities;
using EXILION.Entities.CatchableItems;
using EXILION.Entities.LivingThings;
using EXILION.Items;
using EXILION.World;

#nullable enable

namespace EXILION.Logic;

public sealed class GameWorldLogic
{
    private const float CreatureContactDamageCooldown = 0.8f;
    private float creatureContactDamageTimer;

    public bool TryPlaceStructure(Player player, World.World world, Vector2 worldPosition)
    {
        Item? heldItem = player.rightHand.HeldItem;
        if (heldItem is not Structure structure
            || !player.Inventory.HasItem(ItemRegistry.GetById(heldItem.Id), 1)
            || !world.TryPlaceBlock(worldPosition, player.position, player.GetHitbox().Width / 2f, structure))
        {
            return false;
        }

        player.Inventory.RemoveItem(ItemRegistry.GetById(heldItem.Id), 1);
        return true;
    }

    public bool TryPickupItems(Player player, List<CatchableItem> catchableItems)
    {
        Rectangle playerHitbox = player.GetHitbox();
        bool touchedItem = false;

        for (int i = catchableItems.Count - 1; i >= 0; i--)
        {
            CatchableItem item = catchableItems[i];
            if (!playerHitbox.Intersects(item.GetHitbox())) continue;

            touchedItem = true;
            player.TryPickup(item);
            if (item.Picked) catchableItems.RemoveAt(i);
        }

        return touchedItem;
    }

    public void ProcessCreatureContactDamage(Player player, IReadOnlyList<Creature> creatures, float deltaTime)
    {
        creatureContactDamageTimer = Math.Max(0f, creatureContactDamageTimer - deltaTime);
        if (creatureContactDamageTimer > 0f) return;

        Rectangle playerHitbox = player.GetHitbox();
        foreach (Creature creature in creatures)
        {
            if (creature.isDead || creature.ShouldDespawn || !creature.CanDamagePlayer
                || creature.ContactDamage <= 0 || !playerHitbox.Intersects(creature.GetHitbox()))
            {
                continue;
            }

            player.takeDamage(creature.ContactDamage);
            creatureContactDamageTimer = CreatureContactDamageCooldown;
            return;
        }
    }

    public void ResolvePlayerAttacks(Player player, World.World world, IReadOnlyList<Creature> creatures,
        IReadOnlyList<Player>? otherPlayers = null)
    {
        ApplyHandAttackHits(player, world, creatures, otherPlayers, player.rightHand);
        ApplyHandAttackHits(player, world, creatures, otherPlayers, player.leftHand);
    }

    private static void ApplyHandAttackHits(Player player, World.World world,
        IReadOnlyList<Creature> creatures, IReadOnlyList<Player>? otherPlayers, Hand hand)
    {
        if (hand.CurrentHitbox is not AttackHitbox hitbox) return;

        world.DamageObjects(hitbox, hand.AttackDamage, hand.HeldItem, hand.RegisterHit);

        foreach (Creature creature in creatures)
        {
            if (creature.isDead || !AttackIntersectsCreature(hitbox, creature)) continue;
            if (hand.RegisterHit(creature)) creature.takeDamage(hand.AttackDamage, player.position);
        }

        if (otherPlayers == null) return;
        foreach (Player target in otherPlayers)
        {
            if (ReferenceEquals(player, target) || target.isDead || !AttackIntersectsPlayer(hitbox, target)) continue;
            if (hand.RegisterHit(target)) target.takeDamage(hand.AttackDamage);
        }
    }

    private static bool AttackIntersectsCreature(AttackHitbox attack, Creature creature)
    {
        Vector2 offset = creature.position - attack.Position;
        float cos = MathF.Cos(attack.Rotation);
        float sin = MathF.Sin(attack.Rotation);
        float localX = offset.X * cos + offset.Y * sin;
        float localY = -offset.X * sin + offset.Y * cos;
        float radius = creature.GetHitbox().Width * 0.5f;
        return MathF.Abs(localX) <= attack.Width * 0.5f + radius
            && MathF.Abs(localY) <= attack.Height * 0.5f + radius;
    }

    private static bool AttackIntersectsPlayer(AttackHitbox attack, Player target)
    {
        Vector2 offset = target.position - attack.Position;
        float cos = MathF.Cos(attack.Rotation);
        float sin = MathF.Sin(attack.Rotation);
        float localX = offset.X * cos + offset.Y * sin;
        float localY = -offset.X * sin + offset.Y * cos;
        float radius = target.GetHitbox().Width * 0.5f;
        return MathF.Abs(localX) <= attack.Width * 0.5f + radius
            && MathF.Abs(localY) <= attack.Height * 0.5f + radius;
    }
}
