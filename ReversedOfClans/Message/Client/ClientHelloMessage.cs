using System.Net.Sockets;
using ReversedOfClans.Message.Transmit;

namespace ReversedOfClans.Message.Receive;

public sealed class ClientHelloMessage
{
    private readonly NetworkStream _conn;

    public ClientHelloMessage(byte[] payload, NetworkStream conn)
    {
        _ = payload;
        _conn = conn;
    }

    public Task DecodeAsync() => Task.CompletedTask;

    public Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var msg = new ServerHello(_conn);
        return msg.SendAsync(cancellationToken);
    }
}
