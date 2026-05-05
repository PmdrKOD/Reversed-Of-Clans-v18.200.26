using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using ReversedOfClans.Core;
using ReversedOfClans.Gate;
Logger.Banner();

var server = new TcpListener(
    IPAddress.Parse("192.168.0.103"),
    9339
);
server.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
server.Start();
Logger.Write("server started!");
Socket.StartServer(server);
Console.ReadLine();