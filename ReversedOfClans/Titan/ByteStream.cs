using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace ReversedOfClans.Core;

public sealed class ByteStream
{
    private readonly List<byte> _buffer;
    private int _offset;
    private int _bitOffset;

    public ByteStream()
    {
        _buffer = [];
        _offset = 0;
        _bitOffset = 0;
    }

    public ByteStream(byte[] data)
    {
        _buffer = new List<byte>(data);
        _offset = 0;
        _bitOffset = 0;
    }

    public IReadOnlyList<byte> Buffer => _buffer;
    public byte[] ToArray() => [.. _buffer];

    public int ReadInt()
    {
        _bitOffset = 0;
        int value = BinaryPrimitives.ReadInt32BigEndian(CollectionsMarshal.AsSpan(_buffer).Slice(_offset, 4));
        _offset += 4;
        return value;
    }

    public short ReadShort()
    {
        _bitOffset = 0;
        short value = BinaryPrimitives.ReadInt16BigEndian(CollectionsMarshal.AsSpan(_buffer).Slice(_offset, 2));
        _offset += 2;
        return value;
    }

    public string ReadString()
    {
        int length = ReadInt();
        if (length <= 0 || length >= 90_000)
        {
            return string.Empty;
        }

        string result = Encoding.UTF8.GetString(CollectionsMarshal.AsSpan(_buffer).Slice(_offset, length));
        _offset += length;
        return result;
    }

    public int ReadVInt()
    {
        _bitOffset = 0;
        uint result = 0;
        int shift = 0;

        uint b = _buffer[_offset++];
        uint a1 = (b & 0x40) >> 6;
        uint a2 = (b & 0x80) >> 7;
        uint s = (b << 1) & 0x7E;
        b = s | (a2 << 7) | a1;

        result |= (b & 0x7F) << shift;
        shift += 7;

        while ((b & 0x80) != 0)
        {
            if (shift > 28)
            {
                break;
            }

            b = _buffer[_offset++];
            result |= (b & 0x7F) << shift;
            shift += 7;
        }

        int r = unchecked((int)result);
        return (r >> 1) ^ -(r & 1);
    }

    public bool ReadBoolean() => ReadVInt() >= 1;

    public (int, int) ReadLogicLong() => (ReadVInt(), ReadVInt());
    public (int, int) ReadLong() => (ReadInt(), ReadInt());

    public (int, int) ReadDataReference()
    {
        int a = ReadVInt();
        return (a, a == 0 ? 0 : ReadVInt());
    }

    public void WriteByte(byte value)
    {
        _bitOffset = 0;
        _buffer.Add(value);
        _offset += 1;
    }

    public void WriteShort(short value)
    {
        _bitOffset = 0;
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(bytes, value);
        _buffer.AddRange(bytes.ToArray());
        _offset += 2;
    }

    public void WriteInt(int value)
    {
        _bitOffset = 0;
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        _buffer.AddRange(bytes.ToArray());
        _offset += 4;
    }

    public void WriteVInt(int value)
    {
        _bitOffset = 0;
        uint v = unchecked((uint)value);
        uint flipped = unchecked((uint)(value ^ (value >> 31)));

        uint temp = (v >> 25) & 0x40;
        temp |= v & 0x3F;
        v >>= 6;
        flipped >>= 6;

        if (flipped == 0)
        {
            WriteByte((byte)temp);
            return;
        }

        WriteByte((byte)(temp | 0x80));
        flipped >>= 7;

        uint r = flipped != 0 ? 0x80u : 0u;
        WriteByte((byte)((v & 0x7F) | r));
        v >>= 7;

        while (flipped != 0)
        {
            flipped >>= 7;
            r = flipped != 0 ? 0x80u : 0u;
            WriteByte((byte)((v & 0x7F) | r));
            v >>= 7;
        }
    }

    public void WriteBoolean(bool value)
    {
        if (_bitOffset == 0)
        {
            _buffer.Add(0);
            _offset += 1;
        }

        if (value)
        {
            _buffer[_offset - 1] |= (byte)(1 << _bitOffset);
        }

        _bitOffset = (_bitOffset + 1) & 7;
    }

    public void WriteString(string? value)
    {
        if (value is null)
        {
            WriteInt(-1);
            return;
        }

        byte[] bytes = EncodeStringBytes(value);
        if (bytes.Length > 90_000)
        {
            WriteInt(-1);
            return;
        }

        WriteInt(bytes.Length);
        _buffer.AddRange(bytes);
        _offset += bytes.Length;
    }

    public void WriteStringVInt(string? value)
    {
        if (value is null)
        {
            WriteVInt(0);
            return;
        }

        byte[] bytes = EncodeStringBytes(value);
        WriteVInt(bytes.Length);
        _buffer.AddRange(bytes);
        _offset += bytes.Length;
    }

    public void WriteLong(int v1, int v2)
    {
        WriteInt(v1);
        WriteInt(v2);
    }

    public void WriteLongLong(long value)
    {
        WriteInt((int)(value >> 32));
        WriteInt((int)value);
    }

    public void WriteLogicLong(int v1, int v2)
    {
        WriteVInt(v1);
        WriteVInt(v2);
    }

    public void WriteDataReference(int v1, int v2)
    {
        if (v1 < 1)
        {
            WriteVInt(0);
        }
        else
        {
            WriteVInt(v1);
            WriteVInt(v2);
        }
    }

    public void WriteBytes(byte[]? data)
    {
        if (data is null)
        {
            WriteInt(-1);
            return;
        }

        WriteInt(data.Length);
        _buffer.AddRange(data);
        _offset += data.Length;
    }

    public void WriteHex(string hex)
    {
        if ((hex.Length & 1) != 0)
        {
            throw new ArgumentException("Odd hex length", nameof(hex));
        }

        for (int i = 0; i < hex.Length; i += 2)
        {
            _buffer.Add(Convert.ToByte(hex.Substring(i, 2), 16));
            _offset += 1;
        }
    }

    private static byte[] EncodeStringBytes(string value)
    {
        foreach (char ch in value)
        {
            if (ch > 0x7F)
            {
                return Encoding.Latin1.GetBytes(value);
            }
        }

        return Encoding.UTF8.GetBytes(value);
    }
}
