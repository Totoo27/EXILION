using EXILION.Network;
using System;

namespace EXILION.Network;

public sealed class RemotePlayer
{
    public Guid PlayerId { get; }

    public float X { get; private set; }
    public float Y { get; private set; }

    public RemotePlayer(Guid playerId)
    {
        PlayerId = playerId;
    }

    public void ApplySnapshot(PlayerSnapshot snapshot)
    {
        X = snapshot.X;
        Y = snapshot.Y;
    }
}