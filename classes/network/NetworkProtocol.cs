using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using EXILION.Saving;

namespace EXILION.Network;

/// <summary>Wire-format constants shared by the client and dedicated server.</summary>
public static class NetworkProtocol
{
    public const uint Magic = 0x4E4C5845; // "EXLN" in little endian
    public const byte Version = 1;
    public const int DefaultPort = 27860;
    public const int MaxPlayerNameBytes = 32;
    public const int MaxWorldNameBytes = 64;

    public enum UdpMessage : byte { Discover = 1, DiscoveryResponse = 2, ClientHello = 3, Heartbeat = 4, ConnectionConfirmed = 5 }
    public enum TcpMessage : byte { Connect = 1, Accepted = 2, Rejected = 3 }
}

public sealed record DiscoveredServer(IPEndPoint Endpoint, string WorldId, string WorldName);
public sealed record ConnectionTicket(
    Guid SessionId,
    Guid UdpKey,
    string WorldId,
    string WorldName,
    int WorldSeed,
    WorldSaveData World);

/// <summary>Small, length-bounded packet codec. Invalid packets are ignored by callers.</summary>
public static class NetworkPacketCodec
{
    private const int HeaderSize = 6;

    public static byte[] CreateDiscoveryRequest()
        => CreateHeader(NetworkProtocol.UdpMessage.Discover);

    public static bool IsDiscoveryRequest(ReadOnlySpan<byte> data)
        => HasHeader(data, NetworkProtocol.UdpMessage.Discover) && data.Length == HeaderSize;

    public static byte[] CreateDiscoveryResponse(int port, string worldId, string worldName)
    {
        byte[] id = Encoding.UTF8.GetBytes(worldId);
        byte[] name = Encoding.UTF8.GetBytes(TrimUtf8(worldName, NetworkProtocol.MaxWorldNameBytes));
        if (id.Length > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(worldId));
        byte[] result = new byte[HeaderSize + 2 + 1 + id.Length + 1 + name.Length];
        WriteHeader(result, NetworkProtocol.UdpMessage.DiscoveryResponse);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(HeaderSize), checked((ushort)port));
        result[HeaderSize + 2] = (byte)id.Length;
        id.CopyTo(result, HeaderSize + 3);
        int nameOffset = HeaderSize + 3 + id.Length;
        result[nameOffset] = (byte)name.Length;
        name.CopyTo(result, nameOffset + 1);
        return result;
    }

    public static bool TryReadDiscoveryResponse(ReadOnlySpan<byte> data, IPEndPoint source, out DiscoveredServer? server)
    {
        server = null;
        if (!HasHeader(data, NetworkProtocol.UdpMessage.DiscoveryResponse) || data.Length < HeaderSize + 4) return false;
        int port = BinaryPrimitives.ReadUInt16LittleEndian(data[HeaderSize..]);
        int idLength = data[HeaderSize + 2];
        int nameLengthOffset = HeaderSize + 3 + idLength;
        if (port == 0 || data.Length < nameLengthOffset + 1) return false;
        int nameLength = data[nameLengthOffset];
        if (data.Length != nameLengthOffset + 1 + nameLength) return false;
        string id = Encoding.UTF8.GetString(data.Slice(HeaderSize + 3, idLength));
        string name = Encoding.UTF8.GetString(data.Slice(nameLengthOffset + 1, nameLength));
        server = new DiscoveredServer(new IPEndPoint(source.Address, port), id, name);
        return true;
    }

    public static byte[] CreateClientHello(Guid sessionId, Guid udpKey)
        => CreateSessionPacket(NetworkProtocol.UdpMessage.ClientHello, sessionId, udpKey);

    public static byte[] CreateHeartbeat(Guid sessionId, Guid udpKey)
        => CreateSessionPacket(NetworkProtocol.UdpMessage.Heartbeat, sessionId, udpKey);

    private static byte[] CreateSessionPacket(NetworkProtocol.UdpMessage type, Guid sessionId, Guid udpKey)
    {
        byte[] result = new byte[HeaderSize + 32];
        WriteHeader(result, type);
        sessionId.TryWriteBytes(result.AsSpan(HeaderSize, 16));
        udpKey.TryWriteBytes(result.AsSpan(HeaderSize + 16, 16));
        return result;
    }

    public static bool TryReadClientHello(ReadOnlySpan<byte> data, out Guid sessionId, out Guid udpKey)
        => TryReadSessionPacket(data, NetworkProtocol.UdpMessage.ClientHello, out sessionId, out udpKey);

    public static bool TryReadHeartbeat(ReadOnlySpan<byte> data, out Guid sessionId, out Guid udpKey)
        => TryReadSessionPacket(data, NetworkProtocol.UdpMessage.Heartbeat, out sessionId, out udpKey);

    private static bool TryReadSessionPacket(ReadOnlySpan<byte> data, NetworkProtocol.UdpMessage type, out Guid sessionId, out Guid udpKey)
    {
        sessionId = udpKey = Guid.Empty;
        if (!HasHeader(data, type) || data.Length != HeaderSize + 32) return false;
        sessionId = new Guid(data.Slice(HeaderSize, 16));
        udpKey = new Guid(data.Slice(HeaderSize + 16, 16));
        return sessionId != Guid.Empty && udpKey != Guid.Empty;
    }

    public static byte[] CreateConnectionConfirmed(Guid sessionId)
    {
        byte[] result = new byte[HeaderSize + 16];
        WriteHeader(result, NetworkProtocol.UdpMessage.ConnectionConfirmed);
        sessionId.TryWriteBytes(result.AsSpan(HeaderSize, 16));
        return result;
    }

    public static bool TryReadConnectionConfirmed(ReadOnlySpan<byte> data, out Guid sessionId)
    {
        sessionId = Guid.Empty;
        if (!HasHeader(data, NetworkProtocol.UdpMessage.ConnectionConfirmed) || data.Length != HeaderSize + 16) return false;
        sessionId = new Guid(data.Slice(HeaderSize, 16));
        return sessionId != Guid.Empty;
    }

    public static byte[] CreateHeader(NetworkProtocol.UdpMessage type)
    {
        byte[] packet = new byte[HeaderSize];
        WriteHeader(packet, type);
        return packet;
    }

    private static void WriteHeader(Span<byte> target, NetworkProtocol.UdpMessage type)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(target, NetworkProtocol.Magic);
        target[4] = NetworkProtocol.Version;
        target[5] = (byte)type;
    }

    private static bool HasHeader(ReadOnlySpan<byte> data, NetworkProtocol.UdpMessage type)
        => data.Length >= HeaderSize
        && BinaryPrimitives.ReadUInt32LittleEndian(data) == NetworkProtocol.Magic
        && data[4] == NetworkProtocol.Version && data[5] == (byte)type;

    private static string TrimUtf8(string value, int maxBytes)
    {
        if (Encoding.UTF8.GetByteCount(value) <= maxBytes) return value;
        StringBuilder result = new();
        foreach (char character in value)
        {
            if (Encoding.UTF8.GetByteCount(result.ToString()) + Encoding.UTF8.GetByteCount(character.ToString()) > maxBytes) break;
            result.Append(character);
        }
        return result.ToString();
    }
}

/// <summary>TCP handshake framing shared by both ends of the game connection.</summary>
public static class TcpHandshakeCodec
{
    private const int HeaderSize = 6;
    private const int MaxWorldSnapshotBytes = 4 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    public static byte[] CreateConnectRequest(string playerName)
    {
        byte[] name = Encoding.UTF8.GetBytes(TrimUtf8(playerName.Trim(), NetworkProtocol.MaxPlayerNameBytes));
        if (name.Length == 0) throw new ArgumentException("El nombre del jugador es obligatorio.", nameof(playerName));
        byte[] packet = new byte[HeaderSize + 16 + 2 + name.Length];
        WriteHeader(packet, NetworkProtocol.TcpMessage.Connect);
        Guid.NewGuid().TryWriteBytes(packet.AsSpan(HeaderSize, 16));
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(HeaderSize + 16, 2), (ushort)name.Length);
        name.CopyTo(packet, HeaderSize + 18);
        return packet;
    }

    public static byte[] CreateAccepted(ConnectionTicket ticket)
    {
        byte[] worldId = Encoding.UTF8.GetBytes(ticket.WorldId);
        byte[] worldName = Encoding.UTF8.GetBytes(TrimUtf8(ticket.WorldName, NetworkProtocol.MaxWorldNameBytes));
        byte[] snapshot = JsonSerializer.SerializeToUtf8Bytes(ticket.World, JsonOptions);
        if (worldId.Length > ushort.MaxValue || worldName.Length > ushort.MaxValue || snapshot.Length > MaxWorldSnapshotBytes)
            throw new InvalidOperationException("El estado inicial del mundo es demasiado grande para conectarse.");
        byte[] packet = new byte[HeaderSize + 16 + 16 + 4 + 2 + worldId.Length + 2 + worldName.Length + 4 + snapshot.Length];
        WriteHeader(packet, NetworkProtocol.TcpMessage.Accepted);
        ticket.SessionId.TryWriteBytes(packet.AsSpan(HeaderSize, 16));
        ticket.UdpKey.TryWriteBytes(packet.AsSpan(HeaderSize + 16, 16));
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(HeaderSize + 32, 4), ticket.WorldSeed);
        int offset = HeaderSize + 36;
        WriteBytes(packet, ref offset, worldId);
        WriteBytes(packet, ref offset, worldName);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(offset, 4), snapshot.Length);
        snapshot.CopyTo(packet, offset + 4);
        return packet;
    }

    public static async Task<ConnectionTicket> ReadResponseAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] header = await ReadExactlyAsync(stream, HeaderSize, cancellationToken);
        if (BinaryPrimitives.ReadUInt32LittleEndian(header) != NetworkProtocol.Magic || header[4] != NetworkProtocol.Version)
            throw new InvalidDataException("El servidor respondió con un protocolo incompatible.");
        NetworkProtocol.TcpMessage type = (NetworkProtocol.TcpMessage)header[5];
        if (type == NetworkProtocol.TcpMessage.Rejected)
        {
            int length = BinaryPrimitives.ReadUInt16LittleEndian(await ReadExactlyAsync(stream, 2, cancellationToken));
            if (length > 512) throw new InvalidDataException("El servidor rechazó la conexión con un mensaje inválido.");
            throw new InvalidOperationException(Encoding.UTF8.GetString(await ReadExactlyAsync(stream, length, cancellationToken)));
        }
        if (type != NetworkProtocol.TcpMessage.Accepted)
            throw new InvalidDataException("El servidor respondió con un mensaje inesperado.");

        byte[] fixedPart = await ReadExactlyAsync(stream, 38, cancellationToken);
        Guid sessionId = new(fixedPart.AsSpan(0, 16));
        Guid udpKey = new(fixedPart.AsSpan(16, 16));
        int seed = BinaryPrimitives.ReadInt32LittleEndian(fixedPart.AsSpan(32, 4));
        int idLength = BinaryPrimitives.ReadUInt16LittleEndian(fixedPart.AsSpan(36, 2));
        if (sessionId == Guid.Empty || udpKey == Guid.Empty || idLength is < 1 or > 256)
            throw new InvalidDataException("La aceptación del servidor es inválida.");
        string worldId = Encoding.UTF8.GetString(await ReadExactlyAsync(stream, idLength, cancellationToken));
        int nameLength = BinaryPrimitives.ReadUInt16LittleEndian(await ReadExactlyAsync(stream, 2, cancellationToken));
        if (nameLength is < 1 or > NetworkProtocol.MaxWorldNameBytes)
            throw new InvalidDataException("El nombre de mundo recibido es inválido.");
        string worldName = Encoding.UTF8.GetString(await ReadExactlyAsync(stream, nameLength, cancellationToken));
        int snapshotLength = BinaryPrimitives.ReadInt32LittleEndian(await ReadExactlyAsync(stream, 4, cancellationToken));
        if (snapshotLength is < 2 or > MaxWorldSnapshotBytes)
            throw new InvalidDataException("El estado inicial del mundo es inválido.");
        WorldSaveData? world = JsonSerializer.Deserialize<WorldSaveData>(await ReadExactlyAsync(stream, snapshotLength, cancellationToken), JsonOptions);
        if (world == null || world.WorldId != worldId || world.WorldSeed != seed)
            throw new InvalidDataException("El estado inicial no coincide con el servidor.");
        return new ConnectionTicket(sessionId, udpKey, worldId, worldName, seed, world);
    }

    public static byte[] CreateRejected(string reason)
    {
        byte[] message = Encoding.UTF8.GetBytes(reason);
        byte[] packet = new byte[HeaderSize + 2 + message.Length];
        WriteHeader(packet, NetworkProtocol.TcpMessage.Rejected);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(HeaderSize, 2), checked((ushort)message.Length));
        message.CopyTo(packet, HeaderSize + 2);
        return packet;
    }

    private static async Task<byte[]> ReadExactlyAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        byte[] data = new byte[length];
        await stream.ReadExactlyAsync(data, cancellationToken);
        return data;
    }

    private static void WriteHeader(Span<byte> target, NetworkProtocol.TcpMessage type)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(target, NetworkProtocol.Magic);
        target[4] = NetworkProtocol.Version;
        target[5] = (byte)type;
    }

    private static void WriteBytes(byte[] target, ref int offset, byte[] value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(target.AsSpan(offset, 2), checked((ushort)value.Length));
        offset += 2;
        value.CopyTo(target, offset);
        offset += value.Length;
    }

    private static string TrimUtf8(string value, int maxBytes)
    {
        if (Encoding.UTF8.GetByteCount(value) <= maxBytes) return value;
        StringBuilder result = new();
        foreach (char character in value)
        {
            if (Encoding.UTF8.GetByteCount(result.ToString()) + Encoding.UTF8.GetByteCount(character.ToString()) > maxBytes) break;
            result.Append(character);
        }
        return result.ToString();
    }
}

/// <summary>Client-side LAN discovery. It has no UI dependency and can be used by any client frontend.</summary>
public static class ServerDiscovery
{
    public static async Task<IReadOnlyList<DiscoveredServer>> DiscoverAsync(
        int port = NetworkProtocol.DefaultPort, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (port is < 1 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(port));
        using UdpClient socket = new(AddressFamily.InterNetwork) { EnableBroadcast = true };
        using CancellationTokenSource timeoutCancellation = new(timeout ?? TimeSpan.FromSeconds(2));
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCancellation.Token);
        await socket.SendAsync(NetworkPacketCodec.CreateDiscoveryRequest(),
            new IPEndPoint(IPAddress.Broadcast, port), linked.Token);

        Dictionary<string, DiscoveredServer> results = new(StringComparer.Ordinal);
        try
        {
            while (true)
            {
                UdpReceiveResult received = await socket.ReceiveAsync(linked.Token);
                if (NetworkPacketCodec.TryReadDiscoveryResponse(received.Buffer, received.RemoteEndPoint, out DiscoveredServer? server)
                    && server != null)
                    results[$"{server.Endpoint.Address}:{server.Endpoint.Port}:{server.WorldId}"] = server;
            }
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested && !cancellationToken.IsCancellationRequested) { }
        return results.Values.ToList();
    }
}
