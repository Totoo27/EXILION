using System.Collections.Generic;

namespace EXILION.Items;

public static class ItemRegistry
{
    #nullable enable

    public static readonly Item Madera = new Item(
        id: 1,
        type: ItemType.RESOURCES,
        name: "Madera",
        icon: Assets.Sprites.Tronco,
        twoHanded: true
    );

    public static readonly Item Piedra = new Item(
        id: 2,
        type: ItemType.RESOURCES,
        name: "Piedra",
        icon: Assets.Sprites.Piedra
    );

    public static readonly Consumable AguaPurificada = new Consumable(
        id: 3,
        name: "Agua Purificada",
        statRestore: 20,
        statType: Entities.LivingThings.StatType.Thirst,
        icon: Assets.Sprites.AguaPurificada
    );

    public static readonly Consumable CarneCocinada = new Consumable(
        id: 4,
        name: "Carne Cocinada",
        statRestore: 20,
        statType: Entities.LivingThings.StatType.Hunger,
        icon: Assets.Sprites.CarneCocinada
    );

    public static readonly Consumable OxigenoEmbotellado = new Consumable(
        id: 5,
        name: "Oxígeno Embotellado",
        statRestore: 20,
        statType: Entities.LivingThings.StatType.Oxygen,
        icon: Assets.Sprites.OxigenoEmbotellado
    );

    public static readonly Tool HachaDePiedra = new Tool(
        id: 6,
        name: "Hacha de piedra",
        attackSpeed: 3f,
        damage: 7f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.stoneAxe
    );

    public static readonly Consumable Baya = new Consumable(
        id: 7,
        name: "Baya",
        statRestore: 5,
        statType: Entities.LivingThings.StatType.Hunger,
        icon: Assets.Sprites.Baya
    );

    public static readonly Tool HachaDeMadera = new Tool(
        id: 8,
        name: "Hacha de madera",
        attackSpeed: 3f,
        damage: 5f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.woodenAxe
    );

    public static readonly Tool PicoDeMadera = new Tool(
        id: 9,
        name: "Pico de madera",
        attackSpeed: 2.5f,
        damage: 5f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.woodenPickaxe
    );

    public static readonly Tool PicoDePiedra = new Tool(
        id: 10,
        name: "Pico de piedra",
        attackSpeed: 2.5f,
        damage: 8f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.stonePickaxe
    );

    public static readonly Item Hierro = new Item(
        id: 11,
        type: ItemType.RESOURCES,
        name: "Hierro",
        icon: Assets.Sprites.Hierro
    );

    public static readonly Item Titanio = new Item(
        id: 12,
        type: ItemType.RESOURCES,
        name: "Titanio",
        icon: Assets.Sprites.Titanio
    );

    public static readonly Item Torio = new Item(
        id: 13,
        type: ItemType.RESOURCES,
        name: "Torio",
        icon: Assets.Sprites.Torio
    );

    public static readonly Tool HachaDeHierro = new Tool(
        id: 14,
        name: "Hacha de Hierro",
        attackSpeed: 3.2f,
        damage: 10f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.ironAxe
    );

    public static readonly Tool PicoDeHierro = new Tool(
        id: 15,
        name: "Pico de Hierro",
        attackSpeed: 2.5f,
        damage: 12f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.ironPickaxe
    );

    public static readonly Tool HachaDeTitanio = new Tool(
        id: 16,
        name: "Hacha de Titanio",
        attackSpeed: 3.2f,
        damage: 15f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.titaniumAxe
    );

    public static readonly Tool PicoDeTitanio = new Tool(
        id: 17,
        name: "Pico de Titanio",
        attackSpeed: 2.5f,
        damage: 15f,
        hitboxWidth: 20f,
        hitboxHeight: 60f,
        icon: Assets.Sprites.titaniumPickaxe
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

        // Consumables
        { AguaPurificada.Id, AguaPurificada },
        { CarneCocinada.Id, CarneCocinada },
        { OxigenoEmbotellado.Id, OxigenoEmbotellado },
        { Baya.Id, Baya },

        // Tools
        { HachaDePiedra.Id, HachaDePiedra },
        { PicoDePiedra.Id, PicoDePiedra },
        { HachaDeMadera.Id, HachaDeMadera },
        { PicoDeMadera.Id, PicoDeMadera },
        { HachaDeHierro.Id, HachaDeHierro },
        { PicoDeHierro.Id, PicoDeHierro },
        { HachaDeTitanio.Id, HachaDeTitanio },
        { PicoDeTitanio.Id, PicoDeTitanio }
    };

    public static Item? GetById(int id)
    {
        items.TryGetValue(id, out Item? item);
        return item;
    }

}