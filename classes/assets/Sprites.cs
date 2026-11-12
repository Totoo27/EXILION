using EXILION.Scenes;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EXILION;
public sealed class Sprites
{

    // UI
    public Texture2D MenuBackground { get; private set; }
    public Texture2D GameTitle { get; private set; }
    public Texture2D Sun { get; private set; }
    public Texture2D Button { get; private set; }

    // Player
    public Texture2D Player { get; private set; }
    public Texture2D playerHand { get; private set; }

    // Creatures
    public Texture2D PeacefulCreature { get; private set; }

    // HUD
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
    public Texture2D craftingMenu { get; private set; }

    // Items
    public Texture2D Piedra { get; private set; }
    public Texture2D Hierro { get; private set; }
    public Texture2D Titanio { get; private set; }
    public Texture2D Torio { get; private set; }
    public Texture2D botellaVacia { get; private set; }
    public Texture2D arena { get; private set; }
    public Texture2D silicio { get; private set; }
    public Texture2D chip { get; private set; }
    public Texture2D Tronco { get; private set; }
    public Texture2D AguaPurificada { get; private set; }
    public Texture2D CarneCocinada { get; private set; }
    public Texture2D OxigenoEmbotellado { get; private set; }
    public Texture2D GeneradorO2 { get; private set; }
    public Texture2D Baya { get; private set; }
    // Tools
    public Texture2D woodenAxe { get; private set; }
    public Texture2D woodenPickaxe { get; private set; }
    public Texture2D stoneAxe { get; private set; }
    public Texture2D stonePickaxe { get; private set; }
    public Texture2D ironAxe { get; private set; }
    public Texture2D ironPickaxe { get; private set; }
    public Texture2D titaniumAxe { get; private set; }
    public Texture2D titaniumPickaxe { get; private set; }

    // Inventory
    public Texture2D Slot { get; private set; }

    // World
    public Texture2D Tileset { get; private set; }
    public Texture2D Objects { get; private set; } 

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

        // World
        healthMeter = content.Load<Texture2D>("Sprites/UI/HUD/healthMeter");
        healthProgress = content.Load<Texture2D>("Sprites/UI/HUD/healthProgress");

        oxygenMeter = content.Load<Texture2D>("Sprites/UI/HUD/oxygenMeter");
        oxygenProgress = content.Load<Texture2D>("Sprites/UI/HUD/oxygenProgress");

        watchMeter = content.Load<Texture2D>("Sprites/UI/HUD/watchMeter");
        watchProgress = content.Load<Texture2D>("Sprites/UI/HUD/watchProgress");
        watchSetOff = content.Load<Texture2D>("Sprites/UI/HUD/watchSetOff");
      
      

        // Player
        Player = content.Load<Texture2D>("Sprites/Entities/Player");
        playerHand = content.Load<Texture2D>("Sprites/Entities/playerHand");
        Slot   = content.Load<Texture2D>("Sprites/UI/HUD/slot");
        craftingMenu = content.Load<Texture2D>("Sprites/UI/HUD/craftingMenu");

        // Creatures

        PeacefulCreature = content.Load<Texture2D>("Sprites/Entities/peacefulCreature");
        
        //Items
        Piedra = content.Load<Texture2D>("Sprites/Items/piedra");
        Hierro = content.Load<Texture2D>("Sprites/Items/hierro");
        Titanio = content.Load<Texture2D>("Sprites/Items/titanio");
        Torio = content.Load<Texture2D>("Sprites/Items/torio");
        botellaVacia = content.Load<Texture2D>("Sprites/Items/botellaVacia");
        arena = content.Load<Texture2D>("Sprites/Items/arena");
        silicio = content.Load<Texture2D>("Sprites/Items/silicio");
        chip = content.Load<Texture2D>("Sprites/Items/chip");

        Tronco = content.Load<Texture2D>("Sprites/Items/tronco");
        AguaPurificada = content.Load<Texture2D>("Sprites/Items/AguaPurificada");
        CarneCocinada = content.Load<Texture2D>("Sprites/Items/CarneCocinada");
        OxigenoEmbotellado = content.Load<Texture2D>("Sprites/Items/OxigenoEmbotellado");
        GeneradorO2 = content.Load<Texture2D>("Sprites/Items/GeneradorO2");
        Baya = content.Load<Texture2D>("Sprites/Items/bayas");
      
        // Tools
        woodenAxe = content.Load<Texture2D>("Sprites/Items/woodenAxe");
        woodenPickaxe = content.Load<Texture2D>("Sprites/Items/woodenPickaxe");

        stoneAxe = content.Load<Texture2D>("Sprites/Items/stoneAxe");
        stonePickaxe = content.Load<Texture2D>("Sprites/Items/stonePickaxe");

        ironAxe = content.Load<Texture2D>("Sprites/Items/ironAxe");
        ironPickaxe = content.Load<Texture2D>("Sprites/Items/ironPickaxe");

        titaniumAxe = content.Load<Texture2D>("Sprites/Items/titaniumAxe");
        titaniumPickaxe = content.Load<Texture2D>("Sprites/Items/titaniumPickaxe");
        
        // World Test
        Tileset = content.Load<Texture2D>("Sprites/Tiles/Tileset");
        Objects = content.Load<Texture2D>("Sprites/Tiles/objects");

        await MainLoader.addCompletedTask();

    }
}
