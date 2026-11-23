using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Entities.CatchableItems;
using EXILION.Entities.LivingThings;
using System.Collections.Generic;
using EXILION.Network;
using System;

namespace EXILION.Client;

#nullable enable

/// <summary>MonoGame presentation adapter for entity simulation state.</summary>
public sealed class EntityRenderer
{
    private readonly Texture2D _playerTexture;
    private readonly Texture2D _handTexture;
    private readonly Texture2D _peacefulCreatureTexture;
    private readonly Texture2D _neutralCreatureTexture;
    private readonly Texture2D _hostileCreatureTexture;
    private readonly float _scale;
    private readonly Dictionary<CatchableItem, CatchableItemPulse> _dropPulses = new();

    public EntityRenderer(float scale)
    {
        _scale = scale;
        _playerTexture = Assets.Sprites.Player;
        _handTexture = Assets.Sprites.playerHand;
        _peacefulCreatureTexture = Assets.Sprites.PeacefulCreature;
        _neutralCreatureTexture = Assets.Sprites.NeutralCreature;
        _hostileCreatureTexture = Assets.Sprites.HostileCreature;
    }

    public void DrawDrop(SpriteBatch spriteBatch, CatchableItem item, Texture2D pixel, bool showHitboxes)
    {
        if (item.Picked) return;
        float pulseScale = _dropPulses.TryGetValue(item, out CatchableItemPulse? pulse) ? pulse.Scale : 1f;
        DrawSprite(spriteBatch, ItemVisuals.GetIcon(item.Stack.Item), item.VisualPosition, 0f,
            _scale * 0.8f * pulseScale, Color.White);
        Vector2 visualOffset = item.VisualPosition - item.position;
        DrawEntityHitbox(spriteBatch, pixel, item, showHitboxes, visualOffset);
    }

    public void UpdateDrop(CatchableItem item, float deltaTime)
    {
        if (item.Picked)
        {
            _dropPulses.Remove(item);
            return;
        }

        if (!_dropPulses.TryGetValue(item, out CatchableItemPulse? pulse))
        {
            pulse = new CatchableItemPulse();
            _dropPulses.Add(item, pulse);
        }
        pulse.Update(deltaTime);
    }

    public void PruneDropAnimations(IReadOnlyCollection<CatchableItem> activeItems)
    {
        HashSet<CatchableItem> active = new(activeItems);
        foreach (CatchableItem item in new List<CatchableItem>(_dropPulses.Keys))
            if (!active.Contains(item)) _dropPulses.Remove(item);
    }

    public void DrawPlayer(SpriteBatch spriteBatch, Player player, Texture2D pixel, bool showHitboxes,
        Vector2 positionOffset = default, float networkDamageFeedback = 0f)
    {
        Color color = DamageColor(Math.Max(player.DamageFeedbackRemaining, networkDamageFeedback));
        DrawHand(spriteBatch, player.leftHand, color, pixel, showHitboxes, positionOffset);
        DrawHand(spriteBatch, player.rightHand, color, pixel, showHitboxes, positionOffset);
        DrawSprite(spriteBatch, _playerTexture, player.position + positionOffset, player.VisualAngle, _scale, color);
        if (showHitboxes)
        {
            Rectangle hitbox = player.GetHitbox();
            hitbox.Offset((int)positionOffset.X, (int)positionOffset.Y);
            spriteBatch.Draw(pixel, hitbox, Color.Red);
        }
    }

    public void DrawRemotePlayer(SpriteBatch spriteBatch, Vector2 position)
    {
        DrawSprite(spriteBatch, _playerTexture, position, 0f, _scale, Color.LightBlue);
    }

    public void DrawNetworkPlayer(SpriteBatch spriteBatch, PlayerSnapshot snapshot, Color color, float damageFeedback = 0f)
    {
        color *= DamageColor(damageFeedback);
        DrawSprite(spriteBatch, _handTexture, new Vector2(snapshot.LeftHandX, snapshot.LeftHandY), snapshot.LeftHandAngle, 0.8f, color);
        DrawSprite(spriteBatch, _handTexture, new Vector2(snapshot.RightHandX, snapshot.RightHandY), snapshot.RightHandAngle, 0.8f, color);
        DrawHeldItem(spriteBatch, snapshot.LeftHandItemId, snapshot.LeftHandItemX, snapshot.LeftHandItemY, snapshot.LeftHandItemAngle, color);
        DrawHeldItem(spriteBatch, snapshot.RightHandItemId, snapshot.RightHandItemX, snapshot.RightHandItemY, snapshot.RightHandItemAngle, color);
        DrawSprite(spriteBatch, _playerTexture, new Vector2(snapshot.X, snapshot.Y), snapshot.Rotation, _scale, color);
    }

    public void DrawRemotePlayer(SpriteBatch spriteBatch, RemotePlayer player)
    {
        Color color = Color.LightBlue * DamageColor(player.DamageFeedbackRemaining);
        DrawSprite(spriteBatch, _handTexture, new Vector2(player.LeftHandX, player.LeftHandY), player.LeftHandAngle, 0.8f, color);
        DrawSprite(spriteBatch, _handTexture, new Vector2(player.RightHandX, player.RightHandY), player.RightHandAngle, 0.8f, color);
        DrawHeldItem(spriteBatch, player.LeftHandItemId, player.LeftHandItemX, player.LeftHandItemY, player.LeftHandItemAngle, color);
        DrawHeldItem(spriteBatch, player.RightHandItemId, player.RightHandItemX, player.RightHandItemY, player.RightHandItemAngle, color);
        DrawSprite(spriteBatch, _playerTexture, new Vector2(player.X, player.Y), player.Rotation, _scale, color);
    }

    private void DrawHeldItem(SpriteBatch spriteBatch, int itemId, float x, float y, float angle, Color color)
    {
        EXILION.Items.Item? item = EXILION.Items.ItemRegistry.GetById(itemId);
        if (item == null) return;
        float itemScale = item is EXILION.Items.Tool ? 2.5f : 0.8f;
        DrawSprite(spriteBatch, ItemVisuals.GetIcon(item), new Vector2(x, y), angle, itemScale, color);
    }

    public void DrawCreature(SpriteBatch spriteBatch, Creature creature, Texture2D pixel, bool showHitboxes)
    {
        Texture2D texture = creature.Type switch
        {
            CreatureType.Neutral => _neutralCreatureTexture,
            CreatureType.Hostile => _hostileCreatureTexture,
            _ => _peacefulCreatureTexture
        };
        Vector2 visualOffset = creature.PresentationPosition - creature.position;
        DrawSprite(spriteBatch, texture, creature.PresentationPosition, creature.PresentationAngle, _scale * 0.8f,
            DamageColor(creature.DamageFeedbackRemaining));
        DrawEntityHitbox(spriteBatch, pixel, creature, showHitboxes, visualOffset);
    }

    private void DrawHand(SpriteBatch spriteBatch, Hand hand, Color color, Texture2D pixel, bool showHitboxes,
        Vector2 positionOffset = default)
    {
        DrawSprite(spriteBatch, _handTexture, hand.Position + positionOffset, hand.Angle, 0.8f, color);
        if (hand.HeldItem is not null)
        {
            float itemScale = hand.HeldItem is EXILION.Items.Tool ? 2.5f : 0.8f;
            DrawSprite(spriteBatch, ItemVisuals.GetIcon(hand.HeldItem), hand.ItemPosition + positionOffset,
                hand.ItemAngle, itemScale, color);
        }

        if (showHitboxes && hand.CurrentHitbox is { } hitbox)
        {
            spriteBatch.Draw(pixel, hitbox.Position + positionOffset, null, Color.Red * 0.5f, hitbox.Rotation,
                new Vector2(0.5f, 0.5f), new Vector2(hitbox.Width, hitbox.Height), SpriteEffects.None, 0f);
        }
    }

    private static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float angle, float scale, Color color)
    {
        Sprite sprite = new(texture, scale);
        sprite.Update(angle, position);
        sprite.Draw(spriteBatch, color);
    }

    private static void DrawEntityHitbox(SpriteBatch spriteBatch, Texture2D pixel, EXILION.Entities.Entity entity,
        bool showHitboxes, Vector2 positionOffset = default)
    {
        if (!showHitboxes) return;
        Rectangle hitbox = entity.GetHitbox();
        hitbox.Offset((int)positionOffset.X, (int)positionOffset.Y);
        spriteBatch.Draw(pixel, hitbox, Color.Red);
    }

    private static Color DamageColor(float remaining) => remaining > 0f ? new Color(230, 180, 180) : Color.White;
}
