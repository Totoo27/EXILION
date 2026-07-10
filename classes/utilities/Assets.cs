using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace EXILION;
public static class Assets
{
    public static Song MenuMusic { get; private set; }
    public static Song GameMusic { get; private set; }


    public static void Load(ContentManager content)
    {

        MenuMusic = content.Load<Song>("menuMusic");
        //GameMusic = content.Load<Song>("gameMusic");

    }
}