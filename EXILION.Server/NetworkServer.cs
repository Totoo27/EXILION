using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EXILION.Network;
using EXILION.Saving;

namespace EXILION.Server;

internal sealed class NetworkServer : IAsyncDisposable, IDisposable
{
    private const int TcpConnectFixedSize = 4 + 1 + 1 + 16 + 2;
    private readonly WorldSaveData world;
    private readonly int port;
    private readonly Action<string> log;
    private readonly ConcurrentDictionary<Guid, ClientSession> sessions = new();
    private readonly CancellationTokenSource cancellation = new();
    private UdpClient? udp;
    private TcpListener? tcp;
    private Task? udpTask;
    private readonly GameSession gameSession;
    private Task? simulationTask;
    private Task? tcpTask;

    public NetworkServer(WorldSaveData world, int port, Action<string> log)
    {
        this.world = world;
        this.port = port;
        this.log = log;

        gameSession = new GameSession(world);
    }

    public Task StartAsync()
    {
        // TCP and UDP can safely share a numeric port because they are separate transports.
        udp = new UdpClient(AddressFamily.InterNetwork);
        udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        udp.EnableBroadcast = true;
        tcp = new TcpListener(IPAddress.Any, port);
        tcp.Start();
        udpTask = ReceiveUdpLoopAsync(cancellation.Token);
        tcpTask = AcceptTcpLoopAsync(cancellation.Token);
        simulationTask = RunSimulationLoopAsync(cancellation.Token);
        log($"Alojando '{world.WorldName}' en 0.0.0.0:{port}.");
        return Task.CompletedTask;
    }

    private async Task RunSimulationLoopAsync(CancellationToken token)
    {
        using PeriodicTimer timer = new(
            TimeSpan.FromMilliseconds(16)
        );

        const float deltaTime = 1f / 60f;

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                gameSession.Update(deltaTime);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task ReceiveUdpLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult received = await udp!.ReceiveAsync(token);
                ReadOnlySpan<byte> data = received.Buffer;
                if (NetworkPacketCodec.IsDiscoveryRequest(data))
                {
                    byte[] response = NetworkPacketCodec.CreateDiscoveryResponse(port, world.WorldId, world.WorldName);
                    await udp.SendAsync(response, received.RemoteEndPoint, token);
                    continue;
                }
                bool isHello = NetworkPacketCodec.TryReadClientHello(data, out Guid sessionId, out Guid udpKey);
                bool isHeartbeat = !isHello && NetworkPacketCodec.TryReadHeartbeat(data, out sessionId, out udpKey);
                if ((isHello || isHeartbeat)
                    && sessions.TryGetValue(sessionId, out ClientSession? session)
                    && session.RemoteAddress.Equals(received.RemoteEndPoint.Address)
                    && session.UdpKey == udpKey)
                {
                    session.UdpEndpoint = received.RemoteEndPoint;
                    session.LastSeenUtc = DateTime.UtcNow;
                    if (isHello)
                    {
                        await udp.SendAsync(NetworkPacketCodec.CreateConnectionConfirmed(sessionId), received.RemoteEndPoint, token);
                        log($"UDP confirmado para {session.PlayerName} ({received.RemoteEndPoint}).");
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (Exception exception) { log($"Error UDP: {exception.Message}"); }
    }

    private async Task AcceptTcpLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await tcp!.AcceptTcpClientAsync(token);
                _ = HandleTcpClientAsync(client, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (Exception exception) { log($"Error TCP: {exception.Message}"); }
    }

    private async Task HandleTcpClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            client.ReceiveTimeout = 5000;
            client.SendTimeout = 5000;
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] fixedPart = new byte[TcpConnectFixedSize];
                await stream.ReadExactlyAsync(fixedPart, token);
                if (BinaryPrimitives.ReadUInt32LittleEndian(fixedPart) != NetworkProtocol.Magic
                    || fixedPart[4] != NetworkProtocol.Version
                    || fixedPart[5] != (byte)NetworkProtocol.TcpMessage.Connect)
                {
                    await SendRejectedAsync(stream, "Protocolo incompatible.", token);
                    return;
                }
                int nameLength = BinaryPrimitives.ReadUInt16LittleEndian(fixedPart.AsSpan(22, 2));
                if (nameLength is < 1 or > NetworkProtocol.MaxPlayerNameBytes)
                {
                    await SendRejectedAsync(stream, "Nombre de jugador inválido.", token);
                    return;
                }
                byte[] nameBytes = new byte[nameLength];
                await stream.ReadExactlyAsync(nameBytes, token);
                string playerName = Encoding.UTF8.GetString(nameBytes).Trim();
                if (playerName.Length == 0)
                {
                    await SendRejectedAsync(stream, "Nombre de jugador inválido.", token);
                    return;
                }
                if (client.Client.RemoteEndPoint is not IPEndPoint endpoint)
                    return;
                Guid sessionId = Guid.NewGuid();

                ClientSession session = new(sessionId, Guid.NewGuid(), endpoint.Address, playerName);

                sessions[sessionId] = session;
                gameSession.AddPlayer(sessionId, playerName);
                await stream.WriteAsync(TcpHandshakeCodec.CreateAccepted(
                    new ConnectionTicket(session.Id, session.UdpKey, world.WorldId, world.WorldName, world.WorldSeed, world)), token);
                log($"TCP confirmado para {playerName} ({endpoint.Address}).");
            }
            catch (IOException) { }
            catch (OperationCanceledException) { }
            catch (Exception exception) { log($"Conexión TCP rechazada: {exception.Message}"); }
        }
    }

    private static async Task SendRejectedAsync(NetworkStream stream, string reason, CancellationToken token)
    {
        await stream.WriteAsync(TcpHandshakeCodec.CreateRejected(reason), token);
    }

    public async ValueTask DisposeAsync()
    {
        cancellation.Cancel();
        tcp?.Stop();
        udp?.Dispose();
        Task[] tasks = new Task?[] { udpTask, tcpTask, simulationTask }.Where(task => task != null).Cast<Task>().ToArray();
        if (tasks.Length > 0) await Task.WhenAll(tasks);
        cancellation.Dispose();

    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    private sealed class ClientSession(Guid id, Guid udpKey, IPAddress remoteAddress, string playerName)
    {
        public Guid Id { get; } = id;
        public Guid UdpKey { get; } = udpKey;
        public IPAddress RemoteAddress { get; } = remoteAddress;
        public string PlayerName { get; } = playerName;
        public IPEndPoint? UdpEndpoint { get; set; }
        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    }
}
