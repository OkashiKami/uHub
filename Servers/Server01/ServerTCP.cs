using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace uHub.Networking
{
    using uHub.Entity;

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
            SendWelcomeMessage(client.id);
        }

        public static void RemoveClient(string id)
        {
            if (clients.Count <= 0) return;
            SendLeaveMap(id);
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].id == id) clients.RemoveAt(i);
            }
            Program.Log("Client Removed");
        }

        public static void Send(string id, byte[] data)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].id == id)
                {
                    clients[i].Send(data);
                }
            }
        }
        public static void Send(byte[] data, string id = null, bool excudeSelf = false)
        {
            foreach (Client c in clients)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (c.id != id && excudeSelf) c.Send(data);
                    else if (c.id == id && excudeSelf)
                    {
                        Console.WriteLine("Can't do anything...");
                    }
                }
                else c.Send(data);
            }
        }

        public static void SendJoinMap(string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].clientSocket != null && clients[i].id != id)
                {
                    ByteBuffer buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._JoinMap);
                    buffer.WriteString(clients[i].id);
                    Send(id, buffer.ToArray());
                    buffer.Dispose();

                    buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._JoinMap);
                    buffer.WriteString(id);
                    Send(clients[i].id, buffer.ToArray());
                    buffer.Dispose();
                }
            }
        }
        public static void SendLeaveMap(string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].clientSocket != null && clients[i].id != id)
                {
                    ByteBuffer buffer = new ByteBuffer();
                    buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._LeaveMap);
                    buffer.WriteString(id);
                    clients[i].Send(buffer.ToArray());
                    buffer.Dispose();
                }
            }
        }
        public static void SendWelcomeMessage(string id)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)PacketType._Welcome);
            buffer.WriteString(id);
            buffer.WriteString(string.Format("[{0}] Welcome To the server...", DateTime.Now.ToShortTimeString()));
            Send(id, buffer.ToArray());
            SendJoinMap(id);
            buffer.Dispose();
        }
    }
}
