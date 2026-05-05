

using System.Buffers.Binary;
using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Gate;

public static class Socket
{
    public static async Task StartServer(TcpListener server)
    {
        while (true)
        {
            TcpClient conn;
            try
            {
                conn = await server.AcceptTcpClientAsync();
            }
            catch (Exception err)
            {
                Logger.Print($"accept() failed  {err.Message}");
                continue;
            }

            _ = Task.Run(() => ClientLoop(conn));
        }
    }
    public static async Task ReadExact(NetworkStream stream, byte[] dest, CancellationToken cancellationToken = default)
    {
        int total = 0;
        while (total < dest.Length)
        {
            int n = await stream.ReadAsync(dest.AsMemory(total, dest.Length - total), cancellationToken);
            if (n == 0)
            {
                throw new EndOfStreamException();
            }

            total += n;
        }
    }

    public static async Task ClientLoop(TcpClient conn)
    {
        await using NetworkStream stream = conn.GetStream();
        using (conn)
        {
         
            Logger.Print("New Connect");

            byte[] header = new byte[7];

            while (true)
            {
                try
                {
                    await ReadExact(stream, header);
                }
                catch (Exception err) when (err is EndOfStreamException or IOException or SocketException)
                {
                    Logger.Print("client disconnect");
                    break;
                }

                ushort msgId = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(0, 2));
                int msgLen = (header[2] << 16) | (header[3] << 8) | header[4];
                ushort msgVer = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(5, 2));
                _ = msgVer;

                byte[] payload = new byte[msgLen];
                if (msgLen > 0)
                {
                    try
                    {
                        await ReadExact(stream, payload);
                    }
                    catch (Exception err) when (err is EndOfStreamException or IOException or SocketException)
                    {
                        Logger.Print($"client disconnect : {err}");
                        break;
                    }
                }

            
                Logger.PacketIn(msgId);
                try
                {
                    await MessageFactory.DispatchAsync(msgId, payload, stream);
                }
                catch (Exception err)
                {
                  
                }
            }
        }
    }

}
