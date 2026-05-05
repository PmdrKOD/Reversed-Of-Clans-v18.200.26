using System.Net.Sockets;
using ReversedOfClans.Core;

namespace ReversedOfClans.Message.Transmit;

public sealed class ServerHello : PiranhaMessage
{
    public ServerHello(NetworkStream conn) : base(20100, 0, conn)
    {
    }

    protected override void Encode()
    {
        WriteInt(24);
        for (int i = 0; i < 24; i++)
        {
            WriteByte(1);
        }
    }
}
