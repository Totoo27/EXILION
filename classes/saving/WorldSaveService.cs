using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EXILION.Saving;

public class WorldSaveService
{
    #nullable enable

    private readonly string savesDirectory;

    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public WorldSaveService(string? savesDirectory = null)
    {
        this.savesDirectory = savesDirectory ?? Path.Combine(AppContext.BaseDirectory, "Saves", "worlds");
    }

    public string GetWorldPath(string worldId)
    {
        return Path.Combine(savesDirectory, $"{worldId}.json");
    }

    public async Task<WorldSaveData> CreateWorldAsync(string worldName, int seed)
    {
        string worldId = Guid.NewGuid().ToString("N");

        WorldSaveData world = new()
        {
            WorldId = worldId,
            WorldName = worldName,
            WorldSeed = seed
        };

        await SaveAsync(GetWorldPath(worldId), world);

        return world;
    }

    public async Task SaveAsync(string path, WorldSaveData saveData)
    {
        string? directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(saveData, options);

        await File.WriteAllTextAsync(path, json);
    }

    public async Task<WorldSaveData> LoadAsync(string path)
    {
        string json = await File.ReadAllTextAsync(path);

        WorldSaveData? saveData = JsonSerializer.Deserialize<WorldSaveData>(json, options);

        if (saveData is null)
        {
            throw new InvalidDataException("No se pudieron cargar los datos del mundo.");
        }

        return saveData;
    }

    public async Task<List<WorldSaveData>> GetAllWorldsAsync()
    {
        Directory.CreateDirectory(savesDirectory);

        List<WorldSaveData> worlds = new();

        foreach (string path in Directory.GetFiles(savesDirectory,"*.json"))
        {
            try
            {
                WorldSaveData world = await LoadAsync(path);
                worlds.Add(world);
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException || ex is InvalidDataException)
            {
                // Un archivo dañado no debe impedir
                // la lectura de los demás mundos.
            }
        }

        return worlds.OrderBy(world => world.WorldName).ToList();
    }

    public async Task<WorldSaveData> LoadWorldAsync(string worldId)
    {
        string path = GetWorldPath(worldId);

        WorldSaveData world = await LoadAsync(path);

        if (world.WorldId != worldId)
        {
            throw new InvalidDataException("El ID del mundo no coincide con el archivo.");
        }

        return world;
    }

    public void DeleteWorld(string worldId)
    {
        string path = GetWorldPath(worldId);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }
}