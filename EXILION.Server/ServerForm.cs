using EXILION.Saving;

namespace EXILION.Server;

/// <summary>Deliberately small server-only UI: choose one persisted world, then host it.</summary>
internal sealed class ServerForm : Form
{
    private readonly WorldSaveService saves = new();
    private readonly ServerOptions options;
    private readonly ComboBox worlds = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox name = new() { MaxLength = 32, PlaceholderText = "Nombre del mundo" };
    private readonly NumericUpDown seed = new() { Minimum = int.MinValue, Maximum = int.MaxValue };
    private readonly Button create = new() { Text = "Crear mundo" };
    private readonly Button refresh = new() { Text = "Actualizar" };
    private readonly Button start = new() { Text = "Iniciar servidor" };
    private readonly Label status = new() { AutoSize = false };
    private readonly ListBox log = new();
    private NetworkServer? server;
    private List<WorldSaveData> savedWorlds = [];

    public ServerForm(ServerOptions options)
    {
        this.options = options;
        Text = "EXILION Server";
        ClientSize = new Size(500, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.AddRange([
            Label("Mundos guardados", 20, 20), worlds.At(20, 45, 290), refresh.At(320, 44, 150),
            Label("Nombre", 20, 92), name.At(20, 116, 220), Label("Seed", 255, 92), seed.At(255, 116, 215),
            create.At(20, 157, 220), start.At(255, 157, 215),
            status.At(20, 205, 450, 36), log.At(20, 247, 450, 95)
        ]);
        refresh.Click += async (_, _) => await RefreshWorldsAsync();
        create.Click += async (_, _) => await CreateWorldAsync();
        start.Click += async (_, _) => await StartAsync();
        FormClosing += async (_, _) => { if (server != null) await server.DisposeAsync(); };
        Shown += async (_, _) => await RefreshWorldsAsync();
        SetStatus($"Puerto configurado: {options.Port}. Escucha en todas las IPs.");
    }

    private static Label Label(string text, int x, int y) => new() { Text = text, AutoSize = true, Location = new Point(x, y) };

    private async Task RefreshWorldsAsync()
    {
        try
        {
            savedWorlds = await saves.GetAllWorldsAsync();
            worlds.DataSource = savedWorlds;
            worlds.DisplayMember = nameof(WorldSaveData.WorldName);
            SetStatus(savedWorlds.Count == 0 ? "No hay mundos. Creá uno para iniciar." : "Elegí el mundo a alojar.");
        }
        catch (Exception exception) { SetStatus($"No se pudieron leer los mundos: {exception.Message}"); }
    }

    private async Task CreateWorldAsync()
    {
        string worldName = name.Text.Trim();
        if (worldName.Length == 0) { SetStatus("Ingresá un nombre de mundo."); return; }
        try
        {
            await saves.CreateWorldAsync(worldName, decimal.ToInt32(seed.Value));
            name.Clear();
            await RefreshWorldsAsync();
        }
        catch (Exception exception) { SetStatus($"No se pudo crear el mundo: {exception.Message}"); }
    }

    private async Task StartAsync()
    {
        if (server != null) return;
        if (worlds.SelectedItem is not WorldSaveData world) { SetStatus("Elegí o creá un mundo primero."); return; }
        try
        {
            server = new NetworkServer(world, options.Port, AddLog);
            await server.StartAsync();
            start.Enabled = create.Enabled = refresh.Enabled = worlds.Enabled = false;
            SetStatus($"Servidor activo: UDP y TCP {options.Port}; descubrible por broadcast.");
        }
        catch (Exception exception)
        {
            server?.Dispose(); server = null;
            SetStatus($"No se pudo iniciar en el puerto {options.Port}: {exception.Message}");
        }
    }

    private void SetStatus(string text) => status.Text = text;
    private void AddLog(string text)
    {
        if (IsDisposed) return;
        BeginInvoke(() => { log.Items.Add($"{DateTime.Now:HH:mm:ss}  {text}"); log.TopIndex = log.Items.Count - 1; });
    }
}

internal static class ControlExtensions
{
    public static T At<T>(this T control, int x, int y, int width, int? height = null) where T : Control
    {
        control.Location = new Point(x, y); control.Width = width;
        if (height.HasValue) control.Height = height.Value;
        return control;
    }
}
