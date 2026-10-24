namespace EXILION.World;

public static class WorldHash
{
    public static float Value01(int seed, int x, int y, int salt)
    {
        unchecked
        {
            uint h = (uint)seed;
            h += (uint)x * 374761393u;
            h += (uint)y * 668265263u;
            h += (uint)salt * 2246822519u;
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000; // Da siempre el mismo valor entre 0 y 1 para estas coordenadas.
        }
    }
}
