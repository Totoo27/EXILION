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
    private static readonly TimeSpan SessionInactivityTimeout = TimeSpan.FromSeconds(10);
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
        DateTime nextSessionSweep = DateTime.UtcNow;

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                try
                {
                    gameSession.Update(deltaTime);
                    if (DateTime.UtcNow >= nextSessionSweep)
                    {
                        RemoveInactiveSessions();
                        nextSessionSweep = DateTime.UtcNow + TimeSpan.FromSeconds(1);
                    }
                    if (gameSession.Tick % 3 == 0)
                        await BroadcastSnapshotAsync(token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch (Exception exception) { log($"Error de simulación (se reintentará): {exception.Message}"); }
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
                UdpReceiveResult received;
                try { received = await udp!.ReceiveAsync(token); }
                catch (SocketException exception) when (!token.IsCancellationRequested)
                {
                    // Windows reports ICMP port-unreachable for UDP clients that have exited.
                    log($"Paquete UDP descartado ({exception.SocketErrorCode}); el servidor continúa.");
                    continue;
                }
                ReadOnlySpan<byte> data = received.Buffer;
                if (NetworkPacketCodec.IsDiscoveryRequest(data))
                {
                    byte[] response = NetworkPacketCodec.CreateDiscoveryResponse(port, world.WorldId, world.WorldName);
                    try { await udp.SendAsync(response, received.RemoteEndPoint, token); }
                    catch (SocketException) when (!token.IsCancellationRequested) { }
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
                    session.MarkSeen();
                    if (isHello)
                    {
                        try { await udp.SendAsync(NetworkPacketCodec.CreateConnectionConfirmed(sessionId), received.RemoteEndPoint, token); }
                        catch (SocketException) when (!token.IsCancellationRequested) { }
                        log($"UDP confirmado para {session.PlayerName} ({received.RemoteEndPoint}).");
                    }
                    continue;
                }

                if (NetworkPacketCodec.TryReadPlayerInput(data, out sessionId, out udpKey, out int sequence, out var input, out System.Numerics.Vector2 mousePosition)
                    && sessions.TryGetValue(sessionId, out session)
                    && session.RemoteAddress.Equals(received.RemoteEndPoint.Address)
                    && session.UdpKey == udpKey
                    && session.UdpEndpoint is { } udpEndpoint
                    && udpEndpoint.Equals(received.RemoteEndPoint)
                    && sequence > session.LastInputSequence)
                {
                    session.LastInputSequence = sequence;
                    session.MarkSeen();
                    gameSession.SetPlayerInput(sessionId, input, new Microsoft.Xna.Framework.Vector2(mousePosition.X, mousePosition.Y));
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (Exception exception) { log($"Error UDP: {exception.Message}"); }
    }

    private void RemoveInactiveSessions()
    {
        DateTime now = DateTime.UtcNow;
        foreach (ClientSession session in sessions.Values)
        {
            if (!session.IsInactive(now, SessionInactivityTimeout)
                || !sessions.TryRemove(session.Id, out _))
                continue;

            gameSession.RemovePlayer(session.Id);
            log($"Sesión de {session.PlayerName} cerrada por inactividad.");
        }
    }

    private async Task BroadcastSnapshotAsync(CancellationToken token)
    {
        WorldSnapshot snapshot = gameSession.CreateSnapshot();
        foreach (ClientSession session in sessions.Values)
        {
            IPEndPoint? endpoint = session.UdpEndpoint;
            if (endpoint == null) continue;
            try
            {
                byte[] packet = NetworkPacketCodec.CreateWorldSnapshot(session.Id, session.UdpKey, snapshot);
                await udp!.SendAsync(packet, endpoint, token);
            }
            catch (SocketException) when (!token.IsCancellationRequested) { }
            catch (Exception exception) when (!token.IsCancellationRequested)
            {
                log($"Snapshot no enviado a {session.PlayerName}: {exception.Message}");
            }
        }
    }

    private async Task AcceptTcpLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client;
                try { client = await tcp!.AcceptTcpClientAsync(token); }
                catch (SocketException exception) when (!token.IsCancellationRequested)
                {
                    log($"Conexión TCP descartada ({exception.SocketErrorCode}); el servidor continúa.");
                    continue;
                }
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
        public int LastInputSequence { get; set; }
        private long lastSeenUtcTicks = DateTime.UtcNow.Ticks;

        public void MarkSeen() => Interlocked.Exchange(ref lastSeenUtcTicks, DateTime.UtcNow.Ticks);

        public bool IsInactive(DateTime now, TimeSpan timeout)
            => now.Ticks - Interlocked.Read(ref lastSeenUtcTicks) > timeout.Ticks;
    }
}
