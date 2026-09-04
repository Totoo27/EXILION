namespace EXILION;

using Microsoft.Xna.Framework.Audio;
public static class SFX
{
    
    private static bool enabled = true;

    public static void Play(SoundEffect sound)
    {
        if(!enabled) return;

        sound.Play();
    }

    public static void toggle()
    {
        enabled = !enabled;
    }

}