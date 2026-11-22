using System.Collections.Generic;
using EXILION.World;

namespace EXILION.Items;

public static class ItemRegistry
{
    #nullable enable

    public static readonly Item Madera = new Item(
        id: 1,
        type: ItemType.RESOURCES,
        name: "Wood",
        twoHanded: true
    );

    public static readonly Item Piedra = new Item(
        id: 2,
        type: ItemType.RESOURCES,
        name: "Stone"
    );

    public static readonly Consumable AguaPurificada = new Consumable(
        id: 3,
        name: "Purified Water",
        statRestore: 20,
        statType: Entities.LivingThings.StatType.Thirst
    );

    public static readonly Consumable CarneCocinada = new Consumable(
        id: 4,
        name: "Meat",
        statRestore: 20,
        statType: Entities.LivingThings.StatType.Hunger
    );

    public static readonly Consumable OxigenoEmbotellado = new Consumable(
        id: 5,
        name: "Bottled Oxygen",
        statRestore: 20,
        statType: Entities.LivingThings.StatType.Oxygen
    );

    public static readonly Tool HachaDePiedra = new Tool(
        id: 6,
        name: "Stone Axe",
        attackSpeed: 3f,
        damage: 7f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Consumable Baya = new Consumable(
        id: 7,
        name: "Berry",
        statRestore: 8,
        statType: Entities.LivingThings.StatType.Hunger
    );

    public static readonly Tool HachaDeMadera = new Tool(
        id: 8,
        name: "Wooden Axe",
        attackSpeed: 3f,
        damage: 5f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Tool PicoDeMadera = new Tool(
        id: 9,
        name: "Wooden Pickaxe",
        attackSpeed: 2.5f,
        damage: 5f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Tool PicoDePiedra = new Tool(
        id: 10,
        name: "Stone Pickaxe",
        attackSpeed: 2.5f,
        damage: 8f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Item Hierro = new Item(
        id: 11,
        type: ItemType.RESOURCES,
        name: "Iron"
    );

    public static readonly Item Titanio = new Item(
        id: 12,
        type: ItemType.RESOURCES,
        name: "Titanium"
    );

    public static readonly Item Torio = new Item(
        id: 13,
        type: ItemType.RESOURCES,
        name: "Thorium"
    );

    public static readonly Tool HachaDeHierro = new Tool(
        id: 14,
        name: "Iron Axe",
        attackSpeed: 3.2f,
        damage: 10f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Tool PicoDeHierro = new Tool(
        id: 15,
        name: "Iron Pickaxe",
        attackSpeed: 2.5f,
        damage: 12f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Tool HachaDeTitanio = new Tool(
        id: 16,
        name: "Titanium Axe",
        attackSpeed: 3.2f,
        damage: 15f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Tool PicoDeTitanio = new Tool(
        id: 17,
        name: "Titanium Pickaxe",
        attackSpeed: 2.5f,
        damage: 15f,
        hitboxWidth: 20f,
        hitboxHeight: 60f
    );

    public static readonly Structure GeneradorO2 = new Structure(
        id: 18,
        name: "O2 Generator",
        type: ObjectType.OxygenGenerator
    );

    public static readonly Item BotellaVacia = new Item(
        id: 19,
        type: ItemType.RESOURCES,
        name: "Empty Bottle",
        twoHanded: true
    );

    public static readonly Item Arena = new Item(
        id: 20,
        type: ItemType.RESOURCES,
        name: "Sand",
        twoHanded: false
    );

    public static readonly Item Silicio = new Item(
        id: 21,
        type: ItemType.RESOURCES,
        name: "Silicon",
        twoHanded: false
    );

    public static readonly Item Chip = new Item(
        id: 22,
        type: ItemType.RESOURCES,
        name: "Chip",
        twoHanded: false
    );

    public static readonly Item lowCrystal = new Item(
        id: 23,
        type: ItemType.RESOURCES,
        name: "Low-Grade Crystal",
        twoHanded: false
    );

    public static readonly Item mediumCrystal = new Item(
        id: 24,
        type: ItemType.RESOURCES,
        name: "Mid-Grade Crystal",
        twoHanded: false
    );

    public static readonly Item advancedCrystal = new Item(
        id: 25,
        type: ItemType.RESOURCES,
        name: "Advanced Crystal",
        twoHanded: false
    );

    public static readonly Armor advancedArmor = new Armor(
        id: 26,
        name: "Advanced Armor",
        armorPoints: 60f,
        speedMultiplier: 1.35f
    );

    public static readonly Armor mediumArmor = new Armor(
        id: 27,
        name: "Medium Armor",
        armorPoints: 35f,
        speedMultiplier: 1.25f
    );

    public static readonly Armor lowArmor = new Armor(
        id: 28,
        name: "Low-Grade Armor",
        armorPoints: 20f,
        speedMultiplier: 1.15f
    );

    public static readonly Structure CraftingTable = new Structure(
        id: 29,
        name: "Crafting Table",
        type: ObjectType.CraftingTable
     );

    public static readonly Item turbine = new Item(
        id: 30,
        type: ItemType.RESOURCES,
        name: "Turbine",
        twoHanded: true
    );

    public static readonly Item wing = new Item(
        id: 31,
        type: ItemType.RESOURCES,
        name: "Wing",
        twoHanded: true
    );

    public static readonly Item fuelTank = new Item(
        id: 32,
        type: ItemType.RESOURCES,
        name: "Fuel Tank",
        twoHanded: true
    );

    public static readonly Item cabin = new Item(
        id: 33,
        type: ItemType.RESOURCES,
        name: "Cabin",
        twoHanded: true
    );

    public static readonly Structure FinalShip = new Structure(
        id: 34,
        name: "Spaceship",
        type: ObjectType.SpaceShip
     );

    public static readonly Structure WoodenFloor = new Structure(
        id: 35,
        name: "Wooden Floor",
        type: ObjectType.WoodenFloor
    );

    public static readonly Structure WoodenWall = new Structure(
        id: 36,
        name: "Wooden Wall",
        type: ObjectType.WoodenWall
    );

    public static readonly Structure StoneWall = new Structure(
        id: 37,
        name: "Stone Wall",
        type: ObjectType.StoneWall
    );

    // -------------------------

    private static readonly Dictionary<int, Item> items = new()
    {

        // Resources
        { Madera.Id, Madera },
        { Piedra.Id, Piedra },
        { Hierro.Id, Hierro },
        { Titanio.Id, Titanio },
        { Torio.Id, Torio },
        { BotellaVacia.Id, BotellaVacia },
        { Arena.Id, Arena },
        { Silicio.Id, Silicio },
        { Chip.Id, Chip },
        { lowCrystal.Id, lowCrystal },
        { mediumCrystal.Id, mediumCrystal },
        { advancedCrystal.Id, advancedCrystal },
        { advancedArmor.Id, advancedArmor },
        { mediumArmor.Id, mediumArmor },
        { lowArmor.Id, lowArmor },

        // Consumables
        { AguaPurificada.Id, AguaPurificada },
        { CarneCocinada.Id, CarneCocinada },
        { OxigenoEmbotellado.Id, OxigenoEmbotellado },
        { Baya.Id, Baya },

        // Structures
        { GeneradorO2.Id,GeneradorO2 },
        { CraftingTable.Id, CraftingTable},
        { WoodenFloor.Id, WoodenFloor},
        { WoodenWall.Id, WoodenWall},
        { StoneWall.Id, StoneWall},

        // Tools
        { HachaDePiedra.Id, HachaDePiedra },
        { PicoDePiedra.Id, PicoDePiedra },
        { HachaDeMadera.Id, HachaDeMadera },
        { PicoDeMadera.Id, PicoDeMadera },
        { HachaDeHierro.Id, HachaDeHierro },
        { PicoDeHierro.Id, PicoDeHierro },
        { HachaDeTitanio.Id, HachaDeTitanio },
        { PicoDeTitanio.Id, PicoDeTitanio },

        // Final Ship
        { turbine.Id, turbine },
        { wing.Id, wing },
        { fuelTank.Id, fuelTank },
        { cabin.Id, cabin },
        { FinalShip.Id, FinalShip }
    };

    public static Item? GetById(int id)
    {
        items.TryGetValue(id, out Item? item);
        return item;
    }

    public static Item? GetByObjectType(ObjectType objectType)
    {
        foreach (var item in items.Values)
        {
            if (item is Structure structure && structure.objectType == objectType)
            {
                return structure;
            }
        }
        return null;
    }

}


