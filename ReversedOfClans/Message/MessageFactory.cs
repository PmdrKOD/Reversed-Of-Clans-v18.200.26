using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Message.Receive;

namespace ReversedOfClans.Gate;

public static class MessageFactory
{
    public static async Task DispatchAsync(ushort id, byte[] payload, NetworkStream stream, CancellationToken cancellationToken = default)
    {
        switch (id)
        {
            case 10100:
            {
                var msg = new ClientHelloMessage(payload, stream);
                await msg.DecodeAsync();
                await msg.ProcessAsync(cancellationToken);
                break;
            }
            case 10101:
            {
                var msg = new Login(payload, stream);
                await msg.DecodeAsync();
                await msg.ProcessAsync(cancellationToken);
                break;
            }
            default:
                Logger.PacketNot(id);
                break;
        }
    }
}
