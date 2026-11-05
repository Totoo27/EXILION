namespace EXILION.World;

public static class WorldHash
{
    public static float Value01(int seed, int x, int y, int salt)
    {
        unchecked
        {
            uint hash = (uint)seed;
            hash += (uint)x * 374761393u;
            hash += (uint)y * 668265263u;
            hash += (uint)salt * 2246822519u;
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            hash ^= hash >> 16;
            return (hash & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
