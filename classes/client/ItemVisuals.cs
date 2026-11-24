using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EXILION.Items;

#nullable enable

namespace EXILION.Client;

/// <summary>Resolves client-only item art from stable item identifiers.</summary>
public static class ItemVisuals
{
    public static Texture2D? GetIcon(int itemId) => itemId switch
    {
        1 => Assets.Sprites.Tronco,
        2 => Assets.Sprites.Piedra,
        3 => Assets.Sprites.AguaPurificada,
        4 => Assets.Sprites.CarneCocinada,
        5 => Assets.Sprites.OxigenoEmbotellado,
        6 => Assets.Sprites.stoneAxe,
        7 => Assets.Sprites.Baya,
        8 => Assets.Sprites.woodenAxe,
        9 => Assets.Sprites.woodenPickaxe,
        10 => Assets.Sprites.stonePickaxe,
        11 => Assets.Sprites.Hierro,
        12 => Assets.Sprites.Titanio,
        13 => Assets.Sprites.Torio,
        14 => Assets.Sprites.ironAxe,
        15 => Assets.Sprites.ironPickaxe,
        16 => Assets.Sprites.titaniumAxe,
        17 => Assets.Sprites.titaniumPickaxe,
        18 => Assets.Sprites.GeneradorO2,
        19 => Assets.Sprites.botellaVacia,
        20 => Assets.Sprites.arena,
        21 => Assets.Sprites.silicio,
        22 => Assets.Sprites.chip,
        23 => Assets.Sprites.LowCrystal,
        24 => Assets.Sprites.MediumCrystal,
        25 => Assets.Sprites.AdvancedCrystal,
        26 => Assets.Sprites.AdvancedArmor,
        27 => Assets.Sprites.MediumArmor,
        28 => Assets.Sprites.LowArmor,
        29 => Assets.Sprites.craftingTable,
        30 => Assets.Sprites.turbine,
        31 => Assets.Sprites.wing,
        32 => Assets.Sprites.fuelTank,
        33 => Assets.Sprites.cabin,
        34 => Assets.Sprites.ship,
        35 => Assets.Sprites.woodenFloor,
        36 => Assets.Sprites.woodenWall,
        37 => Assets.Sprites.stoneWall,
        38 => Assets.Sprites.woodenSpear,
        39 => Assets.Sprites.stoneSpear,
        40 => Assets.Sprites.ironSpear,
        _ => null
    };

    public static Texture2D? GetIcon(Item item) => GetIcon(item.Id);

    public static Rectangle? GetIconSource(int itemId) => null;
}
