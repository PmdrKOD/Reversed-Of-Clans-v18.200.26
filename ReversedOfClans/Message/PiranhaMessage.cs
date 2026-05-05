using System.Buffers.Binary;
using System.Net.Sockets;

namespace ReversedOfClans.Core;

public abstract class PiranhaMessage
{
    protected readonly ByteStream Stream;
    private readonly ushort _id;
    private readonly ushort _version;
    private readonly NetworkStream _conn;

    protected PiranhaMessage(ushort id, ushort version, NetworkStream conn)
    {
        Stream = new ByteStream();
        _id = id;
        _version = version;
        _conn = conn;
    }

    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        if (_id < 20_000)
        {
            return;
        }

        Encode();
        byte[] body = Stream.ToArray();
        byte[] header = new byte[7];

        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(0, 2), _id);
        header[2] = (byte)((body.Length >> 16) & 0xFF);
        header[3] = (byte)((body.Length >> 8) & 0xFF);
        header[4] = (byte)(body.Length & 0xFF);
        BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(5, 2), _version);

        await _conn.WriteAsync(header, cancellationToken);
        await _conn.WriteAsync(body, cancellationToken);
        await _conn.FlushAsync(cancellationToken);

    }

    protected abstract void Encode();

    protected void WriteByte(byte value) => Stream.WriteByte(value);
    protected void WriteShort(short value) => Stream.WriteShort(value);
    protected void WriteInt(int value) => Stream.WriteInt(value);
    protected void WriteVInt(int value) => Stream.WriteVInt(value);
    protected void WriteVint(int value) => Stream.WriteVInt(value);
    protected void WriteBoolean(bool value) => Stream.WriteBoolean(value);
    protected void WriteString(string? value) => Stream.WriteString(value);
    protected void WriteStringVInt(string? value) => Stream.WriteStringVInt(value);
    protected void WriteLong(int v1, int v2) => Stream.WriteLong(v1, v2);
    protected void WriteLongLong(long value) => Stream.WriteLongLong(value);
    protected void WriteLogicLong(int v1, int v2) => Stream.WriteLogicLong(v1, v2);
    protected void WriteDataReference(int v1, int v2) => Stream.WriteDataReference(v1, v2);
    protected void WriteBytes(byte[]? data) => Stream.WriteBytes(data);
    protected void WriteHex(string hex) => Stream.WriteHex(hex);
}
