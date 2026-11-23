using EXILION.Network;

namespace EXILION.Server;

internal sealed record ServerOptions(int Port)
{
    public static ServerOptions FromArguments(string[] args)
    {
        for (int index = 0; index < args.Length - 1; index++)
        {
            if ((args[index] is "--port" or "-p") && int.TryParse(args[index + 1], out int port)
                && port is >= 1 and <= ushort.MaxValue)
                return new ServerOptions(port);
        }
        return new ServerOptions(NetworkProtocol.DefaultPort);
    }
}
