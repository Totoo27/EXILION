using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace EXILION;
public sealed class Songs
{

    public Song MenuMusic { get; private set; }
    public void Load(ContentManager content)
    {

        MenuMusic = content.Load<Song>("menuMusic");

    }
}