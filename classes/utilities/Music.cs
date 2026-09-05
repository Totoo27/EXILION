using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;

namespace EXILION;

public static class Music
{
    private static Song currentSong;
    private static Song nextSong;

    private static bool enabled = true;
    private static bool fadeInOut = true;

    private static bool fadingOut = false;
    private static bool fadingIn = false;

    private static float fadeTimer = 0f;
    private static float fadeDuration = 2f;

    public static event Action? musicStop;
    private static bool songEnded = false;

    public static void Play(Song song, float fadeDuration = 0f)
    {
        if (!enabled) return;
        if (song == null) return;

        songEnded = false;

        if (currentSong == song)
        {
            MediaPlayer.Resume();
            return;
        }

        // Cambio instantáneo
        if (!fadeInOut || fadeDuration <= 0f)
        {
            currentSong = song;

            MediaPlayer.Volume = 1f;
            MediaPlayer.IsRepeating = false;
            MediaPlayer.Play(currentSong);

            return;
        }

        // Comenzar transición
        nextSong = song;
        fadeTimer = 0f;
        Music.fadeDuration = fadeDuration;
        fadingOut = true;
        fadingIn = false;
    }

    public static void Update(float deltaTime)
    {
        if (fadingOut)
        {
            fadeTimer += deltaTime;

            float percentage = MathHelper.Clamp(
                fadeTimer / fadeDuration,
                0f,
                1f
            );

            MediaPlayer.Volume = 1f - percentage;

            if (percentage >= 1f)
            {
                currentSong = nextSong;
                nextSong = null;

                MediaPlayer.IsRepeating = false;
                MediaPlayer.Play(currentSong);

                fadeTimer = 0f;

                fadingOut = false;
                fadingIn = true;

                MediaPlayer.Volume = 0f;
            }

            return;
        }

        if (fadingIn)
        {
            fadeTimer += deltaTime;

            float percentage = MathHelper.Clamp(
                fadeTimer / fadeDuration,
                0f,
                1f
            );

            MediaPlayer.Volume = percentage;

            if (percentage >= 1f)
            {
                MediaPlayer.Volume = 1f;
                fadingIn = false;
            }

            return;
        }

        if (MediaPlayer.State == MediaState.Stopped)
        {
            songEnded = true;
         musicStop?.Invoke();
        }
    }

    public static void Stop()
    {
        MediaPlayer.Stop();
        MediaPlayer.Volume = 1f;

        fadingOut = false;
        fadingIn = false;

     musicStop?.Invoke();
    }

    public static void Toggle()
    {
        enabled = !enabled;

        if (enabled)
        {
            Music.Play(currentSong);
        }
        else
        {
            MediaPlayer.Pause();
        }
    }
}