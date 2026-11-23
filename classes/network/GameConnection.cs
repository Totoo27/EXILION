using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using EXILION.Logic;
using System.Diagnostics;

namespace EXILION.Network;

/// <summary>A verified game connection. Dispose it when leaving the multiplayer game.</summary>
public sealed class GameConnection : IAsyncDisposable, IDisposable
{
    private readonly UdpClient udp;
    private readonly IPEndPoint serverEndpoint;
    private readonly CancellationTokenSource cancellation = new();
    private readonly Task heartbeatTask;
    private readonly Task receiveTask;
    private readonly Task inputTask;
    private int inputSequence;
    private PlayerInput currentInput;
    private System.Numerics.Vector2 currentMousePosition;
    private int currentHeldItemId = -1;
    private bool pendingPrimaryAction;
    private bool pendingInteract;
    private bool pendingPlaceBlock;
    private readonly object inputLock = new();
    private readonly ConcurrentQueue<byte[]> pendingGameplayRequests = new();
    private long lastSnapshotTimestamp = Stopwatch.GetTimestamp();
    private long lastSnapshotTick = -1;
    private int dropSequence;
    private int respawnSequence;

    public ConnectionTicket Ticket { get; }
    public IPEndPoint ServerEndpoint => serverEndpoint;
    public bool IsDisconnected => Stopwatch.GetElapsedTime(Interlocked.Read(ref lastSnapshotTimestamp)) > TimeSpan.FromSeconds(5);
    public ConcurrentQueue<WorldSnapshot> Snapshots { get; } = new();


    private GameConnection(UdpClient udp, IPEndPoint serverEndpoint, ConnectionTicket ticket)
    {
        this.udp = udp;
        this.serverEndpoint = serverEndpoint;
        Ticket = ticket;
        heartbeatTask = SendHeartbeatsAsync(cancellation.Token);
        receiveTask = ReceiveSnapshotsAsync(cancellation.Token);
        inputTask = SendInputsAsync(cancellation.Token);
    }

    public static async Task<GameConnection> ConnectAsync(
        IPEndPoint endpoint, string playerName, CancellationToken cancellationToken = default)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(8));
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        using TcpClient tcp = new(AddressFamily.InterNetwork);
        await tcp.ConnectAsync(endpoint, linked.Token);
        NetworkStream stream = tcp.GetStream();
        await stream.WriteAsync(TcpHandshakeCodec.CreateConnectRequest(playerName, PlayerIdentity.GetOrCreate()), linked.Token);
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

    public void SetInput(PlayerInput input, System.Numerics.Vector2 mousePosition, int heldItemId, bool placeBlockRequested)
    {
        lock (inputLock)
        {
            pendingPrimaryAction |= input.PrimaryActionPressed && !input.PrimaryActionCaptured;
            pendingInteract |= input.InteractPressed;
            pendingPlaceBlock |= placeBlockRequested;
            currentInput = input with { PrimaryActionPressed = false, InteractPressed = false };
            currentMousePosition = mousePosition;
            currentHeldItemId = heldItemId;
        }
    }

    public void QueueDropItem(int itemId, int quantity)
    {
        if (itemId < 0 || quantity <= 0) return;
        int sequence = Interlocked.Increment(ref dropSequence);
        pendingGameplayRequests.Enqueue(NetworkPacketCodec.CreateDropItemRequest(
            Ticket.SessionId, Ticket.UdpKey, sequence, itemId, quantity));
    }

    public void QueueRespawnRequest()
    {
        int sequence = Interlocked.Increment(ref respawnSequence);
        pendingGameplayRequests.Enqueue(NetworkPacketCodec.CreateRespawnRequest(
            Ticket.SessionId, Ticket.UdpKey, sequence));
    }

    private async Task SendInputsAsync(CancellationToken token)
    {
        try
        {
            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(33));
            while (await timer.WaitForNextTickAsync(token))
            {
                PlayerInput input;
                System.Numerics.Vector2 mousePosition;
                int heldItemId;
                bool placeBlockRequested;
                lock (inputLock)
                {
                    input = currentInput with
                    {
                        PrimaryActionPressed = pendingPrimaryAction,
                        InteractPressed = pendingInteract
                    };
                    placeBlockRequested = pendingPlaceBlock;
                    pendingPrimaryAction = pendingInteract = pendingPlaceBlock = false;
                    mousePosition = currentMousePosition;
                    heldItemId = currentHeldItemId;
                }
                int sequence = Interlocked.Increment(ref inputSequence);
                byte[] packet = NetworkPacketCodec.CreatePlayerInput(Ticket.SessionId, Ticket.UdpKey, sequence,
                    input, mousePosition, heldItemId, placeBlockRequested);
                try { await udp.SendAsync(packet, serverEndpoint, token); }
                catch (SocketException) when (!token.IsCancellationRequested) { continue; }
                while (pendingGameplayRequests.TryDequeue(out byte[] request))
                {
                    try
                    {
                        for (int attempt = 0; attempt < 3; attempt++)
                            await udp.SendAsync(request, serverEndpoint, token);
                    }
                    catch (SocketException) when (!token.IsCancellationRequested)
                    {
                        pendingGameplayRequests.Enqueue(request);
                        break;
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (SocketException) when (token.IsCancellationRequested) { }
    }

    private async Task ReceiveSnapshotsAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult received = await udp.ReceiveAsync(token);
                if (!received.RemoteEndPoint.Address.Equals(serverEndpoint.Address)
                    || received.RemoteEndPoint.Port != serverEndpoint.Port
                    || !NetworkPacketCodec.TryReadWorldSnapshot(received.Buffer, out Guid sessionId, out Guid udpKey, out WorldSnapshot? snapshot)
                    || sessionId != Ticket.SessionId || udpKey != Ticket.UdpKey || snapshot == null)
                    continue;
                long previousTick = Interlocked.Read(ref lastSnapshotTick);
                if (snapshot.Tick <= previousTick) continue;
                Interlocked.Exchange(ref lastSnapshotTick, snapshot.Tick);
                Snapshots.Enqueue(snapshot);
                Interlocked.Exchange(ref lastSnapshotTimestamp, Stopwatch.GetTimestamp());
                while (Snapshots.Count > 32) Snapshots.TryDequeue(out _);
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        catch (SocketException) when (token.IsCancellationRequested) { }
    }

    public async ValueTask DisposeAsync()
    {
        if (cancellation.IsCancellationRequested) return;
        try
        {
            byte[] disconnect = NetworkPacketCodec.CreateDisconnectRequest(Ticket.SessionId, Ticket.UdpKey);
            for (int attempt = 0; attempt < 3; attempt++)
                await udp.SendAsync(disconnect, serverEndpoint);
        }
        catch (Exception exception) when (exception is SocketException or ObjectDisposedException) { }
        cancellation.Cancel();
        udp.Dispose();
        await heartbeatTask;
        await receiveTask;
        await inputTask;
        cancellation.Dispose();
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
}
