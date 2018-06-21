using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UNetMulti
{
    internal class Server
    {
        private static IAsyncResult result;
        private static IPAddress address;
        private static int port;
        private static TcpListener listener;
        public static List<Client> clients = new List<Client>();
        public Server()
        {
            Console.WriteLine("Please enter a port number or press enter to use default");
            Console.Write("Port (7777): ");
            var tmpPort = Console.ReadLine();
            if (string.IsNullOrEmpty(tmpPort)) { tmpPort = 7777.ToString(); }
            address = IPAddress.Any;
            port = int.Parse(tmpPort);
            listener = new TcpListener(address, port);
            listener.Start();
            Console.WriteLine("Listenering...");
            Console.WriteLine("Waiting for clients");
            listener.BeginAcceptTcpClient(new AsyncCallback(OnClientAccepted), listener);
        }

        private void OnClientAccepted(IAsyncResult ar)
        {
            Console.WriteLine("Client Connected");
            Client c = new Client(listener.EndAcceptTcpClient(ar));
            clients.Add(c);
            SendWelcomeMessage(c.id);
            listener.BeginAcceptTcpClient(new AsyncCallback(OnClientAccepted), listener);
        }

        public static void Send(string id, byte[] data)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].id == id)
                {
                    clients[i].Send(data);
                }
            }
        }
        public static void Send(byte[] data, string id, bool excudeSelf = false)
        {
            foreach (Client c in clients)
            {
                if (c.id != id && excudeSelf) c.Send(data);
                else if (c.id == id && excudeSelf) { /* Do Nothing */ }
            }
        }

        public static void RemoveClient(string id)
        {
            if (clients.Count <= 0) return;
            SendLeaveMap(id);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].id == id)
                    clients.RemoveAt(i);
            }
            Console.WriteLine("Client Removed");
        }
        public static void RemoveClient(Client client)
        {
            if (clients.Count <= 0) return;
            SendLeaveMap(client.id);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].id == client.id)
                    clients.RemoveAt(i);
            }
            Console.WriteLine("Client removed");
        }

        public static void SendJoinMap(string id)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].client != null && clients[i].id != id)
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
                if (clients[i].client != null && clients[i].id != id)
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
            buffer.WriteString("Welcome To the server...");
            Send(id, buffer.ToArray());
            SendJoinMap(id);
            buffer.Dispose();
        }
    }
}