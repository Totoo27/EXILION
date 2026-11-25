using EXILION.Network;
using System;
using Microsoft.Xna.Framework;

namespace EXILION.Network;

public sealed class RemotePlayer
{
    public Guid PlayerId { get; }
    public string Name { get; private set; } = string.Empty;

    public float X { get; private set; }
    public float Y { get; private set; }
    public float Rotation { get; private set; }
    public float LeftHandX { get; private set; }
    public float LeftHandY { get; private set; }
    public float LeftHandAngle { get; private set; }
    public float RightHandX { get; private set; }
    public float RightHandY { get; private set; }
    public float RightHandAngle { get; private set; }
    public int LeftHandItemId { get; private set; } = -1;
    public float LeftHandItemX { get; private set; }
    public float LeftHandItemY { get; private set; }
    public float LeftHandItemAngle { get; private set; }
    public int RightHandItemId { get; private set; } = -1;
    public float RightHandItemX { get; private set; }
    public float RightHandItemY { get; private set; }
    public float RightHandItemAngle { get; private set; }
    public int Health { get; private set; } = 100;
    public float DamageFeedbackRemaining { get; private set; }
    private bool hasReceivedSnapshot;
    private PlayerSnapshot? targetSnapshot;

    public RemotePlayer(Guid playerId)
    {
        PlayerId = playerId;
    }

    public bool ApplySnapshot(PlayerSnapshot snapshot)
    {
        bool tookDamage = hasReceivedSnapshot && snapshot.Health < Health;
        if (tookDamage) DamageFeedbackRemaining = 0.3f;
        if (!hasReceivedSnapshot)
            ApplyVisualSnapshot(snapshot, 1f);
        targetSnapshot = snapshot;
        Health = snapshot.Health;
        hasReceivedSnapshot = true;
        return tookDamage;
    }

    public void Update(float deltaTime)
    {
        DamageFeedbackRemaining = Math.Max(0f, DamageFeedbackRemaining - deltaTime);
        if (targetSnapshot is { } target)
            ApplyVisualSnapshot(target, 1f - MathF.Exp(-24f * deltaTime));
    }

    private void ApplyVisualSnapshot(PlayerSnapshot snapshot, float amount)
    {
        Name = snapshot.PlayerName;
        X = MathHelper.Lerp(X, snapshot.X, amount);
        Y = MathHelper.Lerp(Y, snapshot.Y, amount);
        Rotation = LerpAngle(Rotation, snapshot.Rotation, amount);
        LeftHandX = MathHelper.Lerp(LeftHandX, snapshot.LeftHandX, amount);
        LeftHandY = MathHelper.Lerp(LeftHandY, snapshot.LeftHandY, amount);
        LeftHandAngle = LerpAngle(LeftHandAngle, snapshot.LeftHandAngle, amount);
        RightHandX = MathHelper.Lerp(RightHandX, snapshot.RightHandX, amount);
        RightHandY = MathHelper.Lerp(RightHandY, snapshot.RightHandY, amount);
        RightHandAngle = LerpAngle(RightHandAngle, snapshot.RightHandAngle, amount);
        LeftHandItemId = snapshot.LeftHandItemId;
        LeftHandItemX = MathHelper.Lerp(LeftHandItemX, snapshot.LeftHandItemX, amount);
        LeftHandItemY = MathHelper.Lerp(LeftHandItemY, snapshot.LeftHandItemY, amount);
        LeftHandItemAngle = LerpAngle(LeftHandItemAngle, snapshot.LeftHandItemAngle, amount);
        RightHandItemId = snapshot.RightHandItemId;
        RightHandItemX = MathHelper.Lerp(RightHandItemX, snapshot.RightHandItemX, amount);
        RightHandItemY = MathHelper.Lerp(RightHandItemY, snapshot.RightHandItemY, amount);
        RightHandItemAngle = LerpAngle(RightHandItemAngle, snapshot.RightHandItemAngle, amount);
    }

    private static float LerpAngle(float current, float target, float amount)
        => current + MathF.Atan2(MathF.Sin(target - current), MathF.Cos(target - current)) * amount;
}
