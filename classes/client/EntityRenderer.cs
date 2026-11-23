using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Entities.CatchableItems;
using EXILION.Entities.LivingThings;
using System.Collections.Generic;
using EXILION.Network;

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
        DrawSprite(spriteBatch, ItemVisuals.GetIcon(item.Stack.Item), item.position, 0f, _scale * 0.8f * pulseScale, Color.White);
        DrawEntityHitbox(spriteBatch, pixel, item, showHitboxes);
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

    public void DrawPlayer(SpriteBatch spriteBatch, Player player, Texture2D pixel, bool showHitboxes)
    {
        Color color = DamageColor(player.DamageFeedbackRemaining);
        DrawHand(spriteBatch, player.leftHand, color, pixel, showHitboxes);
        DrawHand(spriteBatch, player.rightHand, color, pixel, showHitboxes);
        DrawSprite(spriteBatch, _playerTexture, player.position, player.VisualAngle, _scale, color);
        DrawEntityHitbox(spriteBatch, pixel, player, showHitboxes);
    }

    public void DrawRemotePlayer(SpriteBatch spriteBatch, Vector2 position)
    {
        DrawSprite(spriteBatch, _playerTexture, position, 0f, _scale, Color.LightBlue);
    }

    public void DrawNetworkPlayer(SpriteBatch spriteBatch, PlayerSnapshot snapshot, Color color)
    {
        DrawSprite(spriteBatch, _handTexture, new Vector2(snapshot.LeftHandX, snapshot.LeftHandY), snapshot.LeftHandAngle, 0.8f, color);
        DrawSprite(spriteBatch, _handTexture, new Vector2(snapshot.RightHandX, snapshot.RightHandY), snapshot.RightHandAngle, 0.8f, color);
        DrawSprite(spriteBatch, _playerTexture, new Vector2(snapshot.X, snapshot.Y), snapshot.Rotation, _scale, color);
    }

    public void DrawRemotePlayer(SpriteBatch spriteBatch, RemotePlayer player)
    {
        DrawNetworkPlayer(spriteBatch, new PlayerSnapshot(player.PlayerId, player.X, player.Y, player.Rotation, 100,
            player.LeftHandX, player.LeftHandY, player.LeftHandAngle,
            player.RightHandX, player.RightHandY, player.RightHandAngle), Color.LightBlue);
    }

    public void DrawCreature(SpriteBatch spriteBatch, Creature creature, Texture2D pixel, bool showHitboxes)
    {
        Texture2D texture = creature.Type switch
        {
            CreatureType.Neutral => _neutralCreatureTexture,
            CreatureType.Hostile => _hostileCreatureTexture,
            _ => _peacefulCreatureTexture
        };
        DrawSprite(spriteBatch, texture, creature.position, creature.VisualAngle, _scale * 0.8f,
            DamageColor(creature.DamageFeedbackRemaining));
        DrawEntityHitbox(spriteBatch, pixel, creature, showHitboxes);
    }

    private void DrawHand(SpriteBatch spriteBatch, Hand hand, Color color, Texture2D pixel, bool showHitboxes)
    {
        DrawSprite(spriteBatch, _handTexture, hand.Position, hand.Angle, 0.8f, color);
        if (hand.HeldItem is not null)
        {
            float itemScale = hand.HeldItem is EXILION.Items.Tool ? 2.5f : 0.8f;
            DrawSprite(spriteBatch, ItemVisuals.GetIcon(hand.HeldItem), hand.ItemPosition, hand.ItemAngle, itemScale, color);
        }

        if (showHitboxes && hand.CurrentHitbox is { } hitbox)
        {
            spriteBatch.Draw(pixel, hitbox.Position, null, Color.Red * 0.5f, hitbox.Rotation,
                new Vector2(0.5f, 0.5f), new Vector2(hitbox.Width, hitbox.Height), SpriteEffects.None, 0f);
        }
    }

    private static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float angle, float scale, Color color)
    {
        Sprite sprite = new(texture, scale);
        sprite.Update(angle, position);
        sprite.Draw(spriteBatch, color);
    }

    private static void DrawEntityHitbox(SpriteBatch spriteBatch, Texture2D pixel, EXILION.Entities.Entity entity, bool showHitboxes)
    {
        if (showHitboxes) spriteBatch.Draw(pixel, entity.GetHitbox(), Color.Red);
    }

    private static Color DamageColor(float remaining) => remaining > 0f ? new Color(230, 180, 180) : Color.White;
}
