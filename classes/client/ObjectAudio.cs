using EXILION.World;

namespace EXILION.Client;

/// <summary>Maps world object IDs to client-only hit effects.</summary>
public static class ObjectAudio
{
    public static void PlayHit(ObjectType type)
    {
        var sound = type switch
        {
            ObjectType.Tree or ObjectType.WoodenFloor or ObjectType.WoodenWall => Assets.SoundEffects?.chop,
            ObjectType.Bush or ObjectType.FruitBush => Assets.SoundEffects?.brokenBush,
            _ => Assets.SoundEffects?.mine
        };

        if (sound != null) SFX.Play(sound);
    }
}
