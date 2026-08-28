using System;

namespace EXILION.Entities.LivingThings;
public struct PlayerStat
{
    public float timer;
    public int value;
    public int max;

    public PlayerStat(int max)
    {
        timer = 0f;
        this.max = max;
        value = this.max;
    }

}