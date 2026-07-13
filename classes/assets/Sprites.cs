using EXILION.Entities.LivingThings;
using EXILION.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;
public sealed class Sprites
{

    public Texture2D MenuBackground { get; private set; }
    public Texture2D GameTitle { get; private set; }
    public Texture2D Button { get; private set; }
    public Texture2D Player { get; private set; }
    public void Load(ContentManager content)
    {

        // UI
        MenuBackground = content.Load<Texture2D>("Sprites/MainMenuBackground");
        GameTitle = content.Load<Texture2D>("Sprites/ExilionTitle");
        Button = content.Load<Texture2D>("Sprites/Button");

        // Player
        Player = content.Load<Texture2D>("Sprites/Player");

    }
}