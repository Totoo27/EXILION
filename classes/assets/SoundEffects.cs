using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EXILION;
public sealed class SoundEffects
{

    public SoundEffect buttonHover { get; private set; }
    public SoundEffect playerDamage { get; private set; }
    public SoundEffect buttonClick { get; private set; }
    public SoundEffect pickUpItem { get; private set; }
    public SoundEffect drink { get; private set; }
    public async Task Load(ContentManager content)
    {

        buttonHover = content.Load<SoundEffect>("SFX/buttonHover");
        playerDamage = content.Load<SoundEffect>("SFX/playerDamage");
        buttonClick = content.Load<SoundEffect>("SFX/buttonClick");
        pickUpItem = content.Load<SoundEffect>("SFX/pickUpItem");
        drink = content.Load<SoundEffect>("SFX/drink");

        await MainLoader.addCompletedTask();
    }
}