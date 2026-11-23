using EXILION.Network;
using System;

namespace EXILION.Network;

public sealed class RemotePlayer
{
    public Guid PlayerId { get; }

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

    public RemotePlayer(Guid playerId)
    {
        PlayerId = playerId;
    }

    public bool ApplySnapshot(PlayerSnapshot snapshot)
    {
        bool tookDamage = hasReceivedSnapshot && snapshot.Health < Health;
        if (tookDamage) DamageFeedbackRemaining = 0.3f;
        X = snapshot.X;
        Y = snapshot.Y;
        Rotation = snapshot.Rotation;
        LeftHandX = snapshot.LeftHandX;
        LeftHandY = snapshot.LeftHandY;
        LeftHandAngle = snapshot.LeftHandAngle;
        RightHandX = snapshot.RightHandX;
        RightHandY = snapshot.RightHandY;
        RightHandAngle = snapshot.RightHandAngle;
        LeftHandItemId = snapshot.LeftHandItemId;
        LeftHandItemX = snapshot.LeftHandItemX;
        LeftHandItemY = snapshot.LeftHandItemY;
        LeftHandItemAngle = snapshot.LeftHandItemAngle;
        RightHandItemId = snapshot.RightHandItemId;
        RightHandItemX = snapshot.RightHandItemX;
        RightHandItemY = snapshot.RightHandItemY;
        RightHandItemAngle = snapshot.RightHandItemAngle;
        Health = snapshot.Health;
        hasReceivedSnapshot = true;
        return tookDamage;
    }

    public void Update(float deltaTime)
        => DamageFeedbackRemaining = Math.Max(0f, DamageFeedbackRemaining - deltaTime);
}
