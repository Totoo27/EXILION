namespace EXILION.Items;

//Clase temporal para prueba de Invetory
public static class ItemRegistry
{
    public static readonly Item Madera = new Item(
        id: 1,
        type: ItemType.RESOURCES,
        name: "Madera",
        icon: Assets.Sprites.Tronco
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

    public static readonly Tool HachaTest = new Tool(
        id: 6,
        name: "Hacha de prueba",
        attackSpeed: 3f,
        damage: 10f,
        hitboxWidth: 20f,
        hitboxHeight: 40f,
        icon: Assets.Sprites.testAxe
    );

}