using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace EXILION;

public static class Music
{
    
    private static Song currentSong;

    private static bool enabled = true;

    public static void Play(Song song)
    {
        if (!enabled) return;

        if (currentSong == song){

            MediaPlayer.Resume();
            return;

        }

        currentSong = song;


        Console.WriteLine("Escuchando: " + currentSong);

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(currentSong);
    }

    public static void Stop()
    {
        MediaPlayer.Stop();
    }

    public static void toggle()
    {
        
        enabled = !enabled;

        if (enabled)
        {
            Music.Play(currentSong);
        } else
        {
            MediaPlayer.Pause();
        }

    }

}