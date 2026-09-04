using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;
using System;

namespace EXILION;
public static class Assets
{
    public static Songs Songs { get; private set; }
    public static Fonts Fonts { get; private set; }
    public static Sprites Sprites { get; private set; }
    public static SoundEffects SoundEffects { get; private set; }
    public static async Task Load(ContentManager content)
    {
        
        Songs = new();
        Fonts = new();
        Sprites = new();
        SoundEffects = new();

        Console.WriteLine("Cargando fonts.");
        await Fonts.Load(content);
        Console.WriteLine("Cargando Canciones.");
        await Songs.Load(content);
        Console.WriteLine("Cargando Sprites.");
        await Sprites.Load(content);
        Console.WriteLine("Cargando Efectos.");
        await SoundEffects.Load(content);

    }
}