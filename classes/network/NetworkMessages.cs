using System;
using System.Numerics;
using EXILION.Logic;
using EXILION.World;
using EXILION.Entities.LivingThings;

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
    int Health,
    PlayerStat Hunger,
    PlayerStat Thirst,
    PlayerStat Oxygen,
    float LeftHandX,
    float LeftHandY,
    float LeftHandAngle,
    float RightHandX,
    float RightHandY,
    float RightHandAngle,
    int LeftHandItemId,
    float LeftHandItemX,
    float LeftHandItemY,
    float LeftHandItemAngle,
    int RightHandItemId,
    float RightHandItemX,
    float RightHandItemY,
    float RightHandItemAngle,
    bool IsDead
);

public sealed record WorldSnapshot(
    int Tick,
    PlayerSnapshot[] Players,
    WorldObjectState[] WorldObjects,
    CreatureSnapshot[] Creatures,
    CatchableItemSnapshot[] CatchableItems,
    WorldObjectHitEvent[] WorldHitEvents,
    int WorldTime,
    int DayCount,
    bool IsNight
);

public sealed record CreatureSnapshot(Guid CreatureId, float X, float Y, float Rotation, int Health, CreatureType Type);

public sealed record CatchableItemSnapshot(Guid ItemId, int ItemTypeId, int Quantity, float X, float Y);

public sealed record WorldObjectHitEvent(int EventId, WorldObjectId ObjectId, ObjectType Type);
