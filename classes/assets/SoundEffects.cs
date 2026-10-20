using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EXILION;
public sealed class SoundEffects
{

    // Buttons
    public SoundEffect buttonHover { get; private set; }
    public SoundEffect buttonClick { get; private set; }

    // Player
    public SoundEffect playerDamage { get; private set; }
    public SoundEffect pickUpItem { get; private set; }
    public SoundEffect[] swings { get; private set; } = new SoundEffect[3];

    // Consume effects
    public SoundEffect drink { get; private set; }
    public SoundEffect eat { get; private set; }
    public SoundEffect bottleBreath { get; private set; }

    public async Task Load(ContentManager content)
    {

        // Buttons
        buttonHover = content.Load<SoundEffect>("SFX/buttonHover");
        buttonClick = content.Load<SoundEffect>("SFX/buttonClick");

        // Player
        playerDamage = content.Load<SoundEffect>("SFX/playerDamage");
        pickUpItem = content.Load<SoundEffect>("SFX/pickUpItem");

        swings[0] = content.Load<SoundEffect>("SFX/Swings/swing1");
        swings[1] = content.Load<SoundEffect>("SFX/Swings/swing2");
        swings[2] = content.Load<SoundEffect>("SFX/Swings/swing3");

        // Consume effects
        drink = content.Load<SoundEffect>("SFX/drink");
        eat = content.Load<SoundEffect>("SFX/eat");
        bottleBreath = content.Load<SoundEffect>("SFX/bottleBreath");

        await MainLoader.addCompletedTask();
    }
}