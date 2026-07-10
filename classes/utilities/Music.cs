using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace EXILION;

public static class Music
{
    
    private static Song currentSong;

    public static void Play(Song song)
    {
        if (currentSong == song)
            return;

        currentSong = song;

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);
    }

    public static void Stop()
    {
        MediaPlayer.Stop();
        currentSong = null;
    }

}