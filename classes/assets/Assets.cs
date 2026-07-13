using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace EXILION;
public static class Assets
{
    public static Songs Songs = new();
    public static Fonts Fonts = new();
    public static Sprites Sprites = new();
    public static void Load(ContentManager content)
    {

        Songs.Load(content);
        Fonts.Load(content);
        Sprites.Load(content);

    }
}