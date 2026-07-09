using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace EXILION;

public static class Music
{
    
    private static Song currentSong;

    public static void PlaySong(string songName, ContentManager content)
    {
        if (currentSong != null)
        {
            MediaPlayer.Stop();
            currentSong.Dispose();
        }

        currentSong = content.Load<Song>(songName);
        MediaPlayer.Play(currentSong);
        MediaPlayer.IsRepeating = true;
    }

    public static void StopSong()
    {
        if (currentSong != null)
        {
            MediaPlayer.Stop();
            currentSong.Dispose();
            currentSong = null;
        }
    }

}