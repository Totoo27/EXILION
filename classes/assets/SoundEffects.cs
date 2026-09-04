using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EXILION;
public sealed class SoundEffects
{

    public SoundEffect buttonHover { get; private set; }
    public SoundEffect playerDamage { get; private set; }
    public async Task Load(ContentManager content)
    {

        buttonHover = content.Load<SoundEffect>("SFX/buttonHover");
        playerDamage = content.Load<SoundEffect>("SFX/playerDamage");

        await MainLoader.addCompletedTask();
    }
}