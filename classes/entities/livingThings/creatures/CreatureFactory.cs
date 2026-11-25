using Microsoft.Xna.Framework;
using System.Collections.Generic;
using EXILION.Items;
using System;
using EXILION.Saving;

namespace EXILION.Entities.LivingThings;

public static class CreatureFactory
{
    #nullable enable

    public static Creature CreateCreature(CreatureType type, Vector2 position, Point hitboxSize, Guid? networkId = null)
    {
        return type switch
        {
            CreatureType.Peaceful => new Creature(
                position,
                new PeacefulCreatureIntelligence(),
                new List<ItemStack> { new(ItemRegistry.CarneCocinada, 1), new(ItemRegistry.lowCrystal, 1) },
                maxHealth: 40, speed: PlayerMovementSettings.GroundSpeed / 2, type: type, contactDamage: 0,
                hitboxSize: hitboxSize, networkId: networkId),

            CreatureType.Neutral => new Creature(
                position,
                new NeutralCreatureIntelligence(),
                new List<ItemStack> { new(ItemRegistry.Silicio, 1), new(ItemRegistry.mediumCrystal, 1) },
                maxHealth: 50, speed: PlayerMovementSettings.GroundSpeed * 3 / 4, type: type, contactDamage: 15,
                hitboxSize: hitboxSize, networkId: networkId),

            CreatureType.Hostile => new Creature(
                position,
                new HostileCreatureIntelligence(),
                new List<ItemStack> { new(ItemRegistry.Hierro, 1), new(ItemRegistry.advancedCrystal, 1) },
                maxHealth: 70, speed: 240, type: type, contactDamage: 10,
                hitboxSize: hitboxSize, networkId: networkId),

            _ => throw new ArgumentException($"Unsupported creature type: {type}")
        };
    }

    public static Creature CreateRestoredCreature(CreatureSaveData saveData, Point hitboxSize)
    {
        Creature creature = CreateCreature(saveData.Type, new Vector2(saveData.PositionX, saveData.PositionY), hitboxSize);
        creature.Restore(saveData);
        return creature;
    }

}
