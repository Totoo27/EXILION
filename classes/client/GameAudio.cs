using EXILION.Logic;
using System;

namespace EXILION.Client;

public static class GameAudio
{
    public static void Play(GameAudioCue cue)
    {
        var sound = cue switch
        {
            GameAudioCue.PlayerDamage => Assets.SoundEffects?.playerDamage,
            GameAudioCue.CreatureDamage => Assets.SoundEffects?.creatureDamage,
            GameAudioCue.Pickup => Assets.SoundEffects?.pickUpItem,
            GameAudioCue.Drink => Assets.SoundEffects?.drink,
            GameAudioCue.Eat => Assets.SoundEffects?.eat,
            GameAudioCue.BottleBreath => Assets.SoundEffects?.bottleBreath,
            GameAudioCue.HandSwing when Assets.SoundEffects?.swings is { Length: > 0 } swings
                => swings[Random.Shared.Next(swings.Length)],
            _ => null
        };

        if (sound != null) SFX.Play(sound);
    }
}
