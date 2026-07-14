using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;

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

        await Fonts.Load(content);
        await Songs.Load(content);
        await Sprites.Load(content);
        await SoundEffects.Load(content);

    }
}