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


    // Objects
    public SoundEffect chop { get; private set; }
    public SoundEffect mine { get; private set; }
    public SoundEffect brokenBush { get; private set; }

    // Consume effects
    public SoundEffect drink { get; private set; }
    public SoundEffect eat { get; private set; }
    public SoundEffect bottleBreath { get; private set; }

    public async Task Load(ContentManager content)
    {

        // Buttons
        buttonHover = content.Load<SoundEffect>("SFX/UI/buttonHover");
        buttonClick = content.Load<SoundEffect>("SFX/UI/buttonClick");

        // Player
        pickUpItem = content.Load<SoundEffect>("SFX/UI/pickUpItem");
        playerDamage = content.Load<SoundEffect>("SFX/damage/playerDamage");

        swings[0] = content.Load<SoundEffect>("SFX/Swings/swing1");
        swings[1] = content.Load<SoundEffect>("SFX/Swings/swing2");
        swings[2] = content.Load<SoundEffect>("SFX/Swings/swing3");

        // Objects
        chop = content.Load<SoundEffect>("SFX/damage/chop");
        mine = content.Load<SoundEffect>("SFX/damage/mine");
        brokenBush = content.Load<SoundEffect>("SFX/damage/brokenBush");

        // Consume effects
        drink = content.Load<SoundEffect>("SFX/consume/drink");
        eat = content.Load<SoundEffect>("SFX/consume/eat");
        bottleBreath = content.Load<SoundEffect>("SFX/consume/bottleBreath");

        await MainLoader.addCompletedTask();
    }
}