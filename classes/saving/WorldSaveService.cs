using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EXILION.Saving;

public class WorldSaveService
{
    #nullable enable

    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true
    };

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

    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}