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
    thirstRestore: 10,
    icon: Assets.Sprites.AguaPurificada
);


}