using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;
public sealed class Sprites
{

    public Texture2D MenuBackground { get; private set; }
    public Texture2D GameTitle { get; private set; }
    public Texture2D Sun { get; private set; }
    public Texture2D Button { get; private set; }
    public Texture2D Player { get; private set; }
    public Texture2D meter { get; private set; }
    public Texture2D meterProgress { get; private set; }
    public Texture2D healthMeter { get; private set; }
    public Texture2D healthProgress { get; private set; }
    public Texture2D oxygenMeter { get; private set; }
    public Texture2D oxygenProgress { get; private set; }
    public Texture2D hungerIcon { get; private set; }
    public Texture2D thirstIcon { get; private set; }
    public Texture2D watchMeter { get; private set; }
    public Texture2D watchProgress { get; private set; }

    public Texture2D Piedra { get; private set; }
    public Texture2D Tronco { get; private set; }

    public Texture2D AguaPurificada { get; private set; }


    public Texture2D Slot { get; private set; }
    // World
    public Texture2D Tileset { get; private set; }

    public async Task Load(ContentManager content)
    {

        // UI
        MenuBackground = content.Load<Texture2D>("Sprites/MainMenuBackground");
        GameTitle = content.Load<Texture2D>("Sprites/ExilionTitle");
        Sun = content.Load<Texture2D>("Sprites/Sun");        
        Button = content.Load<Texture2D>("Sprites/Button");

        // Bars
        meter = content.Load<Texture2D>("Sprites/meter");
        meterProgress = content.Load<Texture2D>("Sprites/meterProgress");
        hungerIcon = content.Load<Texture2D>("Sprites/hungerMeterIcon");
        thirstIcon = content.Load<Texture2D>("Sprites/thirstMeterIcon");

        healthMeter = content.Load<Texture2D>("Sprites/healthMeter");
        healthProgress = content.Load<Texture2D>("Sprites/healthProgress");

        oxygenMeter = content.Load<Texture2D>("Sprites/oxygenMeter");
        oxygenProgress = content.Load<Texture2D>("Sprites/oxygenProgress");

        watchMeter = content.Load<Texture2D>("Sprites/watchMeter");
        watchProgress = content.Load<Texture2D>("Sprites/watchProgress");

        // Player
        Player = content.Load<Texture2D>("Sprites/Player");
        Slot   = content.Load<Texture2D>("Sprites/slot");
        
        //Items
        Piedra = content.Load<Texture2D>("Sprites/piedra");
        Tronco = content.Load<Texture2D>("Sprites/tronco");
        AguaPurificada = content.Load<Texture2D>("Sprites/AguaPurificada");
        
        // World Test
        Tileset = content.Load<Texture2D>("Sprites/Tileset");

        await MainLoader.addCompletedTask();

    }
}