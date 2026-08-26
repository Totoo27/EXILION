using System;

namespace EXILION.Entities.LivingThings;
public struct PlayerStat
{
    public float timer;
    public int value;
    public int max;
    public String name;

    public PlayerStat()
    {
        timer = 0f;
    }

}