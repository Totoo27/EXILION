using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Entities.LivingThings;

namespace EXILION.UI;

public sealed class StatAdviceIndicator
{
    #nullable enable

    private readonly Color criticalColor = new Color(145, 35, 35);
    private readonly Color warningColor = new Color(235, 125, 35);
    private readonly Color cautionColor = new Color(245, 210, 45);

    private readonly Player player;
    private readonly GameContext gameContext;
    private readonly Texture2D frame;
    private readonly Texture2D hungerIcon;
    private readonly Texture2D thirstIcon;
    private readonly Texture2D oxygenIcon;
    private readonly bool[] hungerThresholdsArmed = { true, true, true };
    private readonly bool[] thirstThresholdsArmed = { true, true, true };
    private readonly bool[] oxygenThresholdsArmed = { true, true, true };
    private readonly Queue<(Texture2D icon, Color color)> pendingAdvice = new();
    private Texture2D? activeIcon;
    private Color activeColor = Color.White;
    private float opacity;
    private float displayTimer;
    private static readonly float[] Thresholds = { 0.75f, 0.50f, 0.25f };
    private const float FadeSpeed = 2.5f;
    private const float DisplayDuration = 1.5f;

    public StatAdviceIndicator(Player player, GameContext gameContext)
    {
        this.player = player;
        this.gameContext = gameContext;
        frame = Assets.Sprites.statIndicator;
        hungerIcon = Assets.Sprites.hungerAdvice;
        thirstIcon = Assets.Sprites.thirstAdvice;
        oxygenIcon = Assets.Sprites.oxygenAdvice;
    }

    public void Update(GameTime gameTime)
    {
        float hunger = GetPercent(player.hunger);
        float thirst = GetPercent(player.thirst);
        float oxygen = GetPercent(player.oxygen);

        QueueThresholdAdvice(hunger, hungerIcon, hungerThresholdsArmed);
        QueueThresholdAdvice(thirst, thirstIcon, thirstThresholdsArmed);
        QueueThresholdAdvice(oxygen, oxygenIcon, oxygenThresholdsArmed);

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (displayTimer <= 0f && opacity <= 0f && pendingAdvice.Count > 0)
        {
            (activeIcon, activeColor) = pendingAdvice.Dequeue();
            displayTimer = DisplayDuration;
        }
        displayTimer = Math.Max(0f, displayTimer - elapsed);
        float targetOpacity = displayTimer > 0f ? 1f : 0f;
        float fadeStep = elapsed * FadeSpeed;
        opacity += MathHelper.Clamp(targetOpacity - opacity, -fadeStep, fadeStep);
        if (opacity <= 0f && displayTimer <= 0f) activeIcon = null;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (opacity <= 0f || activeIcon == null) return;

        Rectangle playerBounds = player.GetHitbox();
        float scale = gameContext.ScaleXY(1);
        Vector2 position = new Vector2(
            playerBounds.Right + 28f * scale,
            playerBounds.Top - 22f * scale
        );
        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
        byte alpha = (byte)(255 * opacity);

        spriteBatch.Draw(frame, position, null, activeColor * (alpha / 255f * 0.7f), 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(activeIcon, position, null, Color.White * (alpha / 255f), 0f, new Vector2(activeIcon.Width / 2f, activeIcon.Height / 2f), scale*1.5f, SpriteEffects.None, 0f);
    }

    private static float GetPercent(PlayerStat stat)
    {
        return stat.max <= 0 ? 0f : MathHelper.Clamp(stat.value / (float)stat.max, 0f, 1f);
    }

    private Color GetColor(float percent)
    {
        return percent <= 0.25f ? criticalColor
            : percent <= 0.50f ? warningColor
            : cautionColor;
    }

    private void QueueThresholdAdvice(float percent, Texture2D icon, bool[] armedThresholds)
    {
        for (int i = 0; i < Thresholds.Length; i++)
        {
            float threshold = Thresholds[i];
            if (percent > threshold)
            {
                armedThresholds[i] = true;
            }
            else if (armedThresholds[i])
            {
                armedThresholds[i] = false;
                pendingAdvice.Enqueue((icon, GetColor(threshold)));
            }
        }
    }
}
