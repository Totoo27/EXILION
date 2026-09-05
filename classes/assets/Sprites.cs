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
    public Texture2D watchSetOff { get; private set; }

    public Texture2D Piedra { get; private set; }
    public Texture2D Tronco { get; private set; }

    public Texture2D AguaPurificada { get; private set; }


    public Texture2D Slot { get; private set; }
    // World
    public Texture2D Tileset { get; private set; }

    public async Task Load(ContentManager content)
    {

        // UI
        MenuBackground = content.Load<Texture2D>("Sprites/UI/MainMenuBackground");
        GameTitle = content.Load<Texture2D>("Sprites/UI/ExilionTitle");
        Sun = content.Load<Texture2D>("Sprites/UI/Sun");        
        Button = content.Load<Texture2D>("Sprites/UI/Button");

        // Bars
        meter = content.Load<Texture2D>("Sprites/UI/HUD/meter");
        meterProgress = content.Load<Texture2D>("Sprites/UI/HUD/meterProgress");
        hungerIcon = content.Load<Texture2D>("Sprites/UI/HUD/hungerMeterIcon");
        thirstIcon = content.Load<Texture2D>("Sprites/UI/HUD/thirstMeterIcon");

        healthMeter = content.Load<Texture2D>("Sprites/UI/HUD/healthMeter");
        healthProgress = content.Load<Texture2D>("Sprites/UI/HUD/healthProgress");

        oxygenMeter = content.Load<Texture2D>("Sprites/UI/HUD/oxygenMeter");
        oxygenProgress = content.Load<Texture2D>("Sprites/UI/HUD/oxygenProgress");

        watchMeter = content.Load<Texture2D>("Sprites/UI/HUD/watchMeter");
        watchProgress = content.Load<Texture2D>("Sprites/UI/HUD/watchProgress");
        watchSetOff = content.Load<Texture2D>("Sprites/UI/HUD/watchSetOff");

        // Player
        Player = content.Load<Texture2D>("Sprites/Entities/Player");
        Slot   = content.Load<Texture2D>("Sprites/UI/HUD/slot");
        
        //Items
        Piedra = content.Load<Texture2D>("Sprites/Items/piedra");
        Tronco = content.Load<Texture2D>("Sprites/Items/tronco");
        AguaPurificada = content.Load<Texture2D>("Sprites/Items/AguaPurificada");
        
        // World Test
        Tileset = content.Load<Texture2D>("Sprites/Tiles/Tileset");

        await MainLoader.addCompletedTask();

    }
}