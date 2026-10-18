using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace EXILION;
public sealed class Songs
{

    public Song MenuMusic { get; private set; }
    public Song exiliated { get; private set; }
    public Song drownInInterrogations { get; private set; }
    public async Task Load(ContentManager content)
    {

        MenuMusic = content.Load<Song>("Music/menuMusic");
        exiliated = content.Load<Song>("Music/exiliated");
        drownInInterrogations = content.Load<Song>("Music/drownInInterrogations");

        await MainLoader.addCompletedTask();
    }
}