using System;
using System.Numerics;
using EXILION.Logic;

namespace EXILION.Network;

public sealed record PlayerInputMessage(
    Guid PlayerId,
    int Sequence,
    PlayerInput Input,
    float MouseX,
    float MouseY
);

public sealed record PlayerSnapshot(
    Guid PlayerId,
    float X,
    float Y,
    float Rotation,
    int Health
);

public sealed record WorldSnapshot(
    int Tick,
    PlayerSnapshot[] Players
);