using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;
using System;
using Microsoft.Xna.Framework.Audio;
using EXILION.Scenes;

namespace EXILION;
public static class Assets
{
    public static Songs Songs { get; private set; }
    public static Fonts Fonts { get; private set; }
    public static Sprites Sprites { get; private set; }
    public static SoundEffects SoundEffects { get; private set; }
    public static bool AudioAvaible = true;
    public static async Task Load(ContentManager content)
    {
        
        Songs = new();
        Fonts = new();
        Sprites = new();
        SoundEffects = new();

        Console.WriteLine("Cargando fonts.");
        await Fonts.Load(content);

        try
        {
            Console.WriteLine("Cargando Canciones.");
            await Songs.Load(content);
        }
        catch (NoAudioHardwareException)
        {
            AudioAvaible = false;
            MainLoader.forceCompleteTask();
        }
        
        try
        {
            Console.WriteLine("Cargando Efectos de Sonido.");
            await SoundEffects.Load(content);
        }
        catch (NoAudioHardwareException)
        {
            AudioAvaible = false;
            MainLoader.forceCompleteTask();
        }
        
        Console.WriteLine("Cargando Sprites.");
        await Sprites.Load(content);
        Console.WriteLine("Sprites cargados.");
        

    }
}