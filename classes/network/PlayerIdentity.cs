using System;
using System.IO;

namespace EXILION.Network;

internal static class PlayerIdentity
{
    private static readonly object Sync = new();
    private static Guid? cachedIdentity;

    public static Guid GetOrCreate()
    {
        lock (Sync)
        {
            if (cachedIdentity.HasValue) return cachedIdentity.Value;

            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EXILION");
            string path = Path.Combine(directory, "player-id");
            if (File.Exists(path) && Guid.TryParse(File.ReadAllText(path).Trim(), out Guid existing)
                && existing != Guid.Empty)
            {
                cachedIdentity = existing;
                return existing;
            }

            Directory.CreateDirectory(directory);
            Guid identity = Guid.NewGuid();
            File.WriteAllText(path, identity.ToString("D"));
            cachedIdentity = identity;
            return identity;
        }
    }
}
