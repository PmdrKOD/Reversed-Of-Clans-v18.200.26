using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Message.Transmit;

namespace ReversedOfClans.Message.Receive;

public sealed class Login
{
    private readonly ByteStream _stream;
    private readonly NetworkStream _conn;

    public int HighId { get; private set; }
    public int LowId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public int Major { get; private set; }
    public int Build { get; private set; }
    public int Content { get; private set; }

    public Login(byte[] payload, NetworkStream conn)
    {
        _stream = new ByteStream(payload);
        _conn = conn;
    }

    public Task DecodeAsync()
    {
        HighId = _stream.ReadInt();
        LowId = _stream.ReadInt();
        Token = _stream.ReadString();
        Major = _stream.ReadVInt();
        Build = _stream.ReadVInt();
        Content = _stream.ReadVInt();
        return Task.CompletedTask;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var ok = new LoginOkMessage(_conn);
        await ok.SendAsync(cancellationToken);

        var home = new OwnHomeDataMessage(_conn);
        await home.SendAsync(cancellationToken);
    }
}
