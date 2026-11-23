namespace EXILION.Server;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ServerForm(ServerOptions.FromArguments(args)));
    }
}
