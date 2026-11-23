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

    public RemotePlayer(Guid playerId)
    {
        PlayerId = playerId;
    }

    public void ApplySnapshot(PlayerSnapshot snapshot)
    {
        X = snapshot.X;
        Y = snapshot.Y;
        Rotation = snapshot.Rotation;
        LeftHandX = snapshot.LeftHandX;
        LeftHandY = snapshot.LeftHandY;
        LeftHandAngle = snapshot.LeftHandAngle;
        RightHandX = snapshot.RightHandX;
        RightHandY = snapshot.RightHandY;
        RightHandAngle = snapshot.RightHandAngle;
    }
}
