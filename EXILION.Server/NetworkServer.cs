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
    private const int TcpConnectFixedSize = 4 + 1 + 1 + 16 + 16 + 2;
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
    private readonly WorldSaveService saveService = new();
    private readonly string savePath;
    private readonly SemaphoreSlim saveLock = new(1, 1);
    private Task? simulationTask;
    private Task? tcpTask;

    public NetworkServer(WorldSaveData world, int port, Action<string> log)
    {
        this.world = world;
        this.port = port;
        this.log = log;

        savePath = saveService.GetWorldPath(world.WorldId);
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
                    if (gameSession.Update(deltaTime))
                        await SaveWorldAsync();
                    if (DateTime.UtcNow >= nextSessionSweep)
                    {
                        await RemoveInactiveSessionsAsync();
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
                    && session.UdpKey == udpKey
                    && session.MarkSeen())
                {
                    session.UdpEndpoint = received.RemoteEndPoint;
                    if (isHello)
                    {
                        try { await udp.SendAsync(NetworkPacketCodec.CreateConnectionConfirmed(sessionId), received.RemoteEndPoint, token); }
                        catch (SocketException) when (!token.IsCancellationRequested) { }
                        log($"UDP confirmado para {session.PlayerName} ({received.RemoteEndPoint}).");
                    }
                    continue;
                }

                if (NetworkPacketCodec.TryReadPlayerInput(data, out sessionId, out udpKey, out int sequence,
                    out var input, out System.Numerics.Vector2 mousePosition, out int heldItemId, out bool placeBlockRequested)
                    && sessions.TryGetValue(sessionId, out session)
                    && session.RemoteAddress.Equals(received.RemoteEndPoint.Address)
                    && session.UdpKey == udpKey
                    && session.UdpEndpoint is { } udpEndpoint
                    && udpEndpoint.Equals(received.RemoteEndPoint)
                    && sequence > session.LastInputSequence
                    && session.MarkSeen())
                {
                    session.LastInputSequence = sequence;
                    gameSession.SetPlayerInput(sessionId, input, new Microsoft.Xna.Framework.Vector2(mousePosition.X, mousePosition.Y), heldItemId, placeBlockRequested);
                    continue;
                }

                if (NetworkPacketCodec.TryReadDropItemRequest(data, out sessionId, out udpKey,
                    out int dropSequence, out int itemId, out int quantity)
                    && sessions.TryGetValue(sessionId, out session)
                    && session.RemoteAddress.Equals(received.RemoteEndPoint.Address)
                    && session.UdpKey == udpKey
                    && session.UdpEndpoint is { } dropEndpoint
                    && dropEndpoint.Equals(received.RemoteEndPoint)
                    && session.TryAcceptDropSequence(dropSequence)
                    && session.MarkSeen())
                {
                    gameSession.DropItem(sessionId, itemId, quantity);
                }

                if (NetworkPacketCodec.TryReadRespawnRequest(data, out sessionId, out udpKey, out int respawnSequence)
                    && sessions.TryGetValue(sessionId, out session)
                    && session.RemoteAddress.Equals(received.RemoteEndPoint.Address)
                    && session.UdpKey == udpKey
                    && session.UdpEndpoint is { } respawnEndpoint
                    && respawnEndpoint.Equals(received.RemoteEndPoint)
                    && session.TryAcceptRespawnSequence(respawnSequence)
                    && session.MarkSeen())
                {
                    gameSession.Respawn(sessionId);
                }

                if (NetworkPacketCodec.TryReadDisconnectRequest(data, out sessionId, out udpKey)
                    && sessions.TryGetValue(sessionId, out session)
                    && session.RemoteAddress.Equals(received.RemoteEndPoint.Address)
                    && session.UdpKey == udpKey
                    && session.UdpEndpoint is { } disconnectEndpoint
                    && disconnectEndpoint.Equals(received.RemoteEndPoint)
                    && session.MarkSeen()
                    && sessions.TryRemove(sessionId, out _))
                {
                    gameSession.RemovePlayer(sessionId);
                    log($"SesiÃ³n de {session.PlayerName} cerrada por el jugador.");
                    await SaveWorldAsync();
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (Exception exception) { log($"Error UDP: {exception.Message}"); }
    }

    private async Task RemoveInactiveSessionsAsync()
    {
        DateTime now = DateTime.UtcNow;
        foreach (ClientSession session in sessions.Values)
        {
            if (!session.TryMarkInactive(now, SessionInactivityTimeout)
                || !sessions.TryRemove(session.Id, out _))
                continue;

            gameSession.RemovePlayer(session.Id);
            await SaveWorldAsync();
            log($"Sesión de {session.PlayerName} cerrada por inactividad.");
        }
    }

    private async Task RemovePlayerSessionAsync(ClientSession session, string reason)
    {
        gameSession.RemovePlayer(session.Id);
        await SaveWorldAsync();
        log($"Sesión de {session.PlayerName} {reason}.");
    }

    private async Task SaveWorldAsync()
    {
        await saveLock.WaitAsync();
        try
        {
            await saveService.SaveAsync(savePath, gameSession.CaptureSaveData());
            log("Mundo y perfiles de jugadores guardados.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            log($"No se pudo guardar el mundo: {exception.Message}");
        }
        finally
        {
            saveLock.Release();
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
        ClientSession? pendingSession = null;
        bool handshakeCompleted = false;
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
                Guid persistentPlayerId = new(fixedPart.AsSpan(22, 16));
                int nameLength = BinaryPrimitives.ReadUInt16LittleEndian(fixedPart.AsSpan(38, 2));
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
                if (persistentPlayerId == Guid.Empty)
                {
                    await SendRejectedAsync(stream, "Identidad de jugador inválida.", token);
                    return;
                }
                Guid sessionId = Guid.NewGuid();

                foreach (ClientSession previousSession in sessions.Values
                    .Where(existing => existing.PersistentPlayerId == persistentPlayerId).ToArray())
                {
                    if (!sessions.TryRemove(previousSession.Id, out _)) continue;
                    gameSession.RemovePlayer(previousSession.Id);
                    log($"SesiÃ³n anterior de {playerName} reemplazada al reconectar.");
                }

                ClientSession session = new(sessionId, Guid.NewGuid(), endpoint.Address, playerName, persistentPlayerId);
                pendingSession = session;

                sessions[sessionId] = session;
                gameSession.AddPlayer(sessionId, persistentPlayerId, playerName);
                WorldSaveData currentWorld = gameSession.CaptureSaveData();
                await stream.WriteAsync(TcpHandshakeCodec.CreateAccepted(
                    new ConnectionTicket(session.Id, session.UdpKey, world.WorldId, world.WorldName, world.WorldSeed,
                        currentWorld, gameSession.GetPlayerSaveData(sessionId))), token);
                handshakeCompleted = true;
                log($"TCP confirmado para {playerName} ({endpoint.Address}).");
            }
            catch (IOException)
            {
                if (!handshakeCompleted && pendingSession != null
                    && sessions.TryRemove(pendingSession.Id, out _))
                    await RemovePlayerSessionAsync(pendingSession, "retirada tras fallar el handshake TCP");
            }
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
        await SaveWorldAsync();
        cancellation.Dispose();

    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    private sealed class ClientSession(Guid id, Guid udpKey, IPAddress remoteAddress, string playerName, Guid persistentPlayerId)
    {
        public Guid Id { get; } = id;
        public Guid UdpKey { get; } = udpKey;
        public IPAddress RemoteAddress { get; } = remoteAddress;
        public string PlayerName { get; } = playerName;
        public Guid PersistentPlayerId { get; } = persistentPlayerId;
        public IPEndPoint? UdpEndpoint { get; set; }
        public int LastInputSequence { get; set; }
        private readonly HashSet<int> acceptedDropSequences = new();
        private readonly Queue<int> acceptedDropSequenceOrder = new();
        private int lastRespawnSequence;
        private readonly object activityLock = new();
        private long lastSeenUtcTicks = DateTime.UtcNow.Ticks;
        private bool inactive;

        public bool MarkSeen()
        {
            lock (activityLock)
            {
                if (inactive) return false;
                lastSeenUtcTicks = DateTime.UtcNow.Ticks;
                return true;
            }
        }

        public bool TryMarkInactive(DateTime now, TimeSpan timeout)
        {
            lock (activityLock)
            {
                if (inactive || now.Ticks - lastSeenUtcTicks <= timeout.Ticks) return false;
                inactive = true;
                return true;
            }
        }

        public bool TryAcceptDropSequence(int sequence)
        {
            if (!acceptedDropSequences.Add(sequence)) return false;
            acceptedDropSequenceOrder.Enqueue(sequence);
            if (acceptedDropSequenceOrder.Count > 512)
                acceptedDropSequences.Remove(acceptedDropSequenceOrder.Dequeue());
            return true;
        }

        public bool TryAcceptRespawnSequence(int sequence)
        {
            if (sequence <= lastRespawnSequence) return false;
            lastRespawnSequence = sequence;
            return true;
        }
    }
}
