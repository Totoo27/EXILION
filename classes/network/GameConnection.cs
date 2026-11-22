using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace EXILION.Network;

/// <summary>A verified game connection. Dispose it when leaving the multiplayer game.</summary>
public sealed class GameConnection : IAsyncDisposable, IDisposable
{
    private readonly UdpClient udp;
    private readonly IPEndPoint serverEndpoint;
    private readonly CancellationTokenSource cancellation = new();
    private readonly Task heartbeatTask;

    public ConnectionTicket Ticket { get; }
    public IPEndPoint ServerEndpoint => serverEndpoint;
    public ConcurrentQueue<WorldSnapshot> Snapshots { get; } = new();


    private GameConnection(UdpClient udp, IPEndPoint serverEndpoint, ConnectionTicket ticket)
    {
        this.udp = udp;
        this.serverEndpoint = serverEndpoint;
        Ticket = ticket;
        heartbeatTask = SendHeartbeatsAsync(cancellation.Token);
    }

    public static async Task<GameConnection> ConnectAsync(
        IPEndPoint endpoint, string playerName, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(8));
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        using TcpClient tcp = new(AddressFamily.InterNetwork);
        await tcp.ConnectAsync(endpoint, linked.Token);
        NetworkStream stream = tcp.GetStream();
        await stream.WriteAsync(TcpHandshakeCodec.CreateConnectRequest(playerName), linked.Token);
        ConnectionTicket ticket = await TcpHandshakeCodec.ReadResponseAsync(stream, linked.Token);

        UdpClient udp = new(AddressFamily.InterNetwork);
        try
        {
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            byte[] hello = NetworkPacketCodec.CreateClientHello(ticket.SessionId, ticket.UdpKey);
            for (int attempt = 0; attempt < 3; attempt++)
            {
                await udp.SendAsync(hello, endpoint, linked.Token);
                using CancellationTokenSource confirmationTimeout = new(TimeSpan.FromSeconds(1));
                using CancellationTokenSource confirmation = CancellationTokenSource.CreateLinkedTokenSource(linked.Token, confirmationTimeout.Token);
                try
                {
                    while (true)
                    {
                        UdpReceiveResult received = await udp.ReceiveAsync(confirmation.Token);
                        if (received.RemoteEndPoint.Address.Equals(endpoint.Address)
                            && NetworkPacketCodec.TryReadConnectionConfirmed(received.Buffer, out Guid confirmed)
                            && confirmed == ticket.SessionId)
                            return new GameConnection(udp, endpoint, ticket);
                    }
                }
                catch (OperationCanceledException) when (confirmationTimeout.IsCancellationRequested && !linked.IsCancellationRequested) { }
            }
            throw new TimeoutException("El servidor no confirmó el canal UDP.");
        }
        catch
        {
            udp.Dispose();
            throw;
        }
    }

    private async Task SendHeartbeatsAsync(CancellationToken token)
    {
        try
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(3));
            while (await timer.WaitForNextTickAsync(token))
                await udp.SendAsync(NetworkPacketCodec.CreateHeartbeat(Ticket.SessionId, Ticket.UdpKey), serverEndpoint, token);
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (SocketException) when (token.IsCancellationRequested) { }
    }

    public async ValueTask DisposeAsync()
    {
        cancellation.Cancel();
        udp.Dispose();
        await heartbeatTask;
        cancellation.Dispose();
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}
