using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace EXILION;
public static class Assets
{
    public static Songs Songs { get; private set; }
    public static Fonts Fonts { get; private set; }
    public static Sprites Sprites { get; private set; }
    public static SoundEffects SoundEffects { get; private set; }
    public static void Load(ContentManager content)
    {
        
        Songs = new();
        Fonts = new();
        Sprites = new();
        SoundEffects = new();

        Songs.Load(content);
        Fonts.Load(content);
        Sprites.Load(content);
        SoundEffects.Load(content);

    }
}