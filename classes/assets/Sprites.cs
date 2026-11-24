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
    public Texture2D cursorTexture { get; private set; }

    public Texture2D statIndicator { get; private set; }
    public Texture2D hungerAdvice { get; private set; }
    public Texture2D thirstAdvice { get; private set; }
    public Texture2D oxygenAdvice { get; private set; }

    // Player
    public Texture2D Player { get; private set; }
    public Texture2D playerHand { get; private set; }

    // Creatures
    public Texture2D PeacefulCreature { get; private set; }
    public Texture2D NeutralCreature { get; private set; }
    public Texture2D HostileCreature { get; private set; }

    // Creature drops

    public Texture2D LowCrystal { get; private set; }
    public Texture2D MediumCrystal { get; private set; }
    public Texture2D AdvancedCrystal { get; private set; }

    // Armors

    public Texture2D LowArmor { get; private set; }
    public Texture2D MediumArmor { get; private set; }
    public Texture2D AdvancedArmor { get; private set; }

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
    public Texture2D woodenSpear { get; private set; }
    public Texture2D stoneAxe { get; private set; }
    public Texture2D stonePickaxe { get; private set; }
    public Texture2D stoneSpear { get; private set; }
    public Texture2D ironAxe { get; private set; }
    public Texture2D ironPickaxe { get; private set; }
    public Texture2D ironSpear { get; private set; }
    public Texture2D titaniumAxe { get; private set; }
    public Texture2D titaniumPickaxe { get; private set; }

     public Texture2D craftingTable { get;private set;}

    // Inventory
    public Texture2D Slot { get; private set; }

    // Structures

    public Texture2D woodenFloor { get; private set; }
    public Texture2D woodenWall { get; private set; }
    public Texture2D stoneWall { get; private set; }

    // Final Ship

    public Texture2D turbine { get; private set; }
    public Texture2D wing { get; private set; }
    public Texture2D fuelTank { get; private set; }
    public Texture2D cabin { get; private set; }
    public Texture2D ship { get; private set; }

    // World
    public Texture2D Tileset { get; private set; }
    public Texture2D Objects { get; private set; } 

    public async Task Load(ContentManager content)
    {

        // UI
        MainLoader.SetLoadingText("Sprites: UI");
        MenuBackground = content.Load<Texture2D>("Sprites/UI/MainMenuBackground");
        GameTitle = content.Load<Texture2D>("Sprites/UI/ExilionTitle");
        Sun = content.Load<Texture2D>("Sprites/UI/Sun");        
        Button = content.Load<Texture2D>("Sprites/UI/Button");
        cursorTexture = content.Load<Texture2D>("Sprites/UI/pointer");
        MainLoader.addCompletedTask();

        // Advices
        MainLoader.SetLoadingText("Sprites: UI advices");
        statIndicator = content.Load<Texture2D>("Sprites/UI/HUD/statIndicator");
        hungerAdvice = content.Load<Texture2D>("Sprites/UI/HUD/hungerAdviceIcon");
        thirstAdvice = content.Load<Texture2D>("Sprites/UI/HUD/waterAdviceIcon");
        oxygenAdvice = content.Load<Texture2D>("Sprites/UI/HUD/oxygenAdviceIcon");
        MainLoader.addCompletedTask();

        // Bars
        MainLoader.SetLoadingText("Sprites: UI Bars");
        meter = content.Load<Texture2D>("Sprites/UI/HUD/meter");
        meterProgress = content.Load<Texture2D>("Sprites/UI/HUD/meterProgress");
        hungerIcon = content.Load<Texture2D>("Sprites/UI/HUD/hungerMeterIcon");
        thirstIcon = content.Load<Texture2D>("Sprites/UI/HUD/thirstMeterIcon");
        MainLoader.addCompletedTask();

        // World
        MainLoader.SetLoadingText("Sprites: UI HUD");
        healthMeter = content.Load<Texture2D>("Sprites/UI/HUD/healthMeter");
        healthProgress = content.Load<Texture2D>("Sprites/UI/HUD/healthProgress");
        

        oxygenMeter = content.Load<Texture2D>("Sprites/UI/HUD/oxygenMeter");
        oxygenProgress = content.Load<Texture2D>("Sprites/UI/HUD/oxygenProgress");

        watchMeter = content.Load<Texture2D>("Sprites/UI/HUD/watchMeter");
        watchProgress = content.Load<Texture2D>("Sprites/UI/HUD/watchProgress");
        watchSetOff = content.Load<Texture2D>("Sprites/UI/HUD/watchSetOff");
        MainLoader.addCompletedTask();

        // Player
        MainLoader.SetLoadingText("Sprites: Player");
        Player = content.Load<Texture2D>("Sprites/Entities/Player");
        playerHand = content.Load<Texture2D>("Sprites/Entities/playerHand");
        Slot   = content.Load<Texture2D>("Sprites/UI/HUD/slot");
        craftingMenu = content.Load<Texture2D>("Sprites/UI/HUD/craftingMenu");
        MainLoader.addCompletedTask();

        // Creatures
        MainLoader.SetLoadingText("Sprites: Creatures");
        PeacefulCreature = content.Load<Texture2D>("Sprites/Entities/peacefulCreature");
        HostileCreature = content.Load<Texture2D>("Sprites/Entities/hostileCreature");
        NeutralCreature = content.Load<Texture2D>("Sprites/Entities/neutralCreature");
        MainLoader.addCompletedTask();

        // Creature drops
        MainLoader.SetLoadingText("Sprites: Items creatures");
        LowCrystal = content.Load<Texture2D>("Sprites/Items/lowCrystal");
        MediumCrystal = content.Load<Texture2D>("Sprites/Items/mediumCrystal");
        AdvancedCrystal = content.Load<Texture2D>("Sprites/Items/advancedCrystal");
        MainLoader.addCompletedTask();

        // Armors
        MainLoader.SetLoadingText("Sprites: Items armor");
        LowArmor = content.Load<Texture2D>("Sprites/Items/lowArmor");
        MediumArmor = content.Load<Texture2D>("Sprites/Items/mediumArmor");
        AdvancedArmor = content.Load<Texture2D>("Sprites/Items/advancedArmor");
        MainLoader.addCompletedTask();

        //Items
        MainLoader.SetLoadingText("Sprites: Items resources");
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
        craftingTable= content.Load<Texture2D>("Sprites/Items/craftingTable");
        MainLoader.addCompletedTask();
      
        // Tools
        MainLoader.SetLoadingText("Sprites: Items tools");

        woodenAxe = content.Load<Texture2D>("Sprites/Items/woodenAxe");
        woodenPickaxe = content.Load<Texture2D>("Sprites/Items/woodenPickaxe");
        woodenSpear = content.Load<Texture2D>("Sprites/Items/WoodenSpear");

        stoneAxe = content.Load<Texture2D>("Sprites/Items/stoneAxe");
        stonePickaxe = content.Load<Texture2D>("Sprites/Items/stonePickaxe");
        stoneSpear = content.Load<Texture2D>("Sprites/Items/StoneSpear");

        ironAxe = content.Load<Texture2D>("Sprites/Items/ironAxe");
        ironPickaxe = content.Load<Texture2D>("Sprites/Items/ironPickaxe");
        ironSpear = content.Load<Texture2D>("Sprites/Items/IronSpear");

        titaniumAxe = content.Load<Texture2D>("Sprites/Items/titaniumAxe");
        titaniumPickaxe = content.Load<Texture2D>("Sprites/Items/titaniumPickaxe");
        MainLoader.addCompletedTask();

        // Structures
        MainLoader.SetLoadingText("Sprites: Items structures");
        woodenFloor = content.Load<Texture2D>("Sprites/Items/woodenFloor");
        woodenWall = content.Load<Texture2D>("Sprites/Items/WoodenWall");
        stoneWall = content.Load<Texture2D>("Sprites/Items/StoneWall");
        MainLoader.addCompletedTask();
        
        // Final Ship
        MainLoader.SetLoadingText("Sprites: Items ship");
        turbine = content.Load<Texture2D>("Sprites/Items/turbina");
        wing = content.Load<Texture2D>("Sprites/Items/alas");
        fuelTank = content.Load<Texture2D>("Sprites/Items/combustible");
        cabin = content.Load<Texture2D>("Sprites/Items/cabina");

        ship = content.Load<Texture2D>("Sprites/Items/nave");
        MainLoader.addCompletedTask();

        // World
        MainLoader.SetLoadingText("Sprites: World objects");
        Tileset = content.Load<Texture2D>("Sprites/Tiles/Tileset");
        Objects = content.Load<Texture2D>("Sprites/Tiles/objects");

        MainLoader.addCompletedTask();

    }
}
