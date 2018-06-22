using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uHub
{
    using uHub.Entities;

    class ServerTCP
    {
        public static TcpListener serverSocket;
        public static List<Client> clients = new List<Client>(Constants.MAX_PLAYERS);

        public static void InitializeNetwork()
        {
            serverSocket = new TcpListener(IPAddress.Any, Constants.DEFAULT_PORT);
            serverSocket.Start();
            serverSocket.BeginAcceptSocket(new AsyncCallback(OnClientConnect), null);

            Program.Log("Server has successfully started!");
            Program.Log("Now listening on {0}", serverSocket.Server.LocalEndPoint);
        }
        private static void OnClientConnect(IAsyncResult ar)
        {
            TcpClient clientSocket = serverSocket.EndAcceptTcpClient(ar);
            if(clients.Count < clients.Capacity)
                serverSocket.BeginAcceptSocket(new AsyncCallback(OnClientConnect), null);
            clientSocket.NoDelay = false;
            Client client = new Client(clientSocket);
            clients.Add(client);
            Program.Log("Connection recieved from {0}", client.ip);
            Proxy.SendWelcomeMessage(client.id);
        }

        public static void RemoveClient(string id)
        {
            if (clients.Count <= 0) return;
            Proxy.SendLeaveMap(id);
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].id == id) clients.RemoveAt(i);
            }
            Program.Log("Client Removed");
        }

        
    }
}
