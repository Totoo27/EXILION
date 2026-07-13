using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace EXILION;
public sealed class SoundEffects
{

    public SoundEffect buttonHover { get; private set; }
    public void Load(ContentManager content)
    {

        buttonHover = content.Load<SoundEffect>("SFX/buttonHover");

    }
}