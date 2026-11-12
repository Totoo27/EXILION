using Microsoft.Xna.Framework;
using System.Collections.Generic;
using EXILION.Items;
using System;
using EXILION.Saving;

namespace EXILION.Entities.LivingThings;

public static class CreatureFactory
{
    #nullable enable

    public static Creature CreateCreature(CreatureType type, Vector2 position, GameContext gameContext)
    {
        return type switch
        {
            CreatureType.Peaceful => new Creature(
                position, new Sprite(Assets.Sprites.PeacefulCreature, gameContext.ScaleXY(1) * 0.8f), gameContext,
                new PeacefulCreatureIntelligence(),
                new List<ItemStack> { new(ItemRegistry.CarneCocinada, 1), new(ItemRegistry.lowCrystal, 1) },
                maxHealth: 40, speed: PlayerMovementSettings.GroundSpeed / 2, type: type),

            CreatureType.Neutral => new Creature(
                position, new Sprite(Assets.Sprites.NeutralCreature, gameContext.ScaleXY(1) * 0.8f), gameContext,
                new NeutralCreatureIntelligence(),
                new List<ItemStack> { new(ItemRegistry.CarneCocinada, 1), new(ItemRegistry.mediumCrystal, 1) },
                maxHealth: 50, speed: PlayerMovementSettings.GroundSpeed / 2, type: type),

            CreatureType.Hostile => new Creature(
                position, new Sprite(Assets.Sprites.HostileCreature, gameContext.ScaleXY(1) * 0.8f), gameContext,
                new HostileCreatureIntelligence(),
                new List<ItemStack> { new(ItemRegistry.CarneCocinada, 1), new(ItemRegistry.advancedCrystal, 1) },
                maxHealth: 70, speed: 210, type: type),

            _ => throw new ArgumentException($"Unsupported creature type: {type}")
        };
    }

    public static Creature CreateRestoredCreature(CreatureSaveData saveData, GameContext gameContext)
    {
        Creature creature = CreateCreature(saveData.Type, new Vector2(saveData.PositionX, saveData.PositionY), gameContext);
        creature.Restore(saveData);
        return creature;
    }

}
