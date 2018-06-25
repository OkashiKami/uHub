using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uHub
{
    using uHub.Entities;
    using uHub.Utils;

    class ServerTCP
    {
        public static TcpListener socket;
        public static List<Client> clients = new List<Client>(Constants.MAX_PLAYERS);

        public static void InitializeNetwork()
        {
            socket = new TcpListener(IPAddress.Any, Constants.DEFAULT_PORT);
            socket.Start();
            socket.BeginAcceptSocket(new AsyncCallback(OnClientConnect), null);

            Program.Log("Server has successfully started!");
            Program.Log("Now listening on {0}", socket.Server.LocalEndPoint);
        }
        private static void OnClientConnect(IAsyncResult ar)
        {
            TcpClient socket = ServerTCP.socket.EndAcceptTcpClient(ar);
            socket.NoDelay = false;
            Client client = new Client(socket);
            clients.Add(client);
            if (clients.Count < clients.Capacity)
                ServerTCP.socket.BeginAcceptSocket(new AsyncCallback(OnClientConnect), null);
            else Program.Log("Server client list is full");
        }
        
    }
}
