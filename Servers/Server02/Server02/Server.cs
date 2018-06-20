using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server02
{
    internal class Server
    {
        private static TcpListener net;
        private static Thread serverThread;
        public static List<Client> clients = new List<Client>();

        internal static void Init()
        {
            net = new TcpListener(IPAddress.Any, 7777);
            serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.Start();
        }
        private static void StartServer()
        {
            Console.WriteLine("Starting Server");
            net.Start();
            while(true)
            {
                if (net.Pending()) Console.WriteLine("Pending connection awaiting...");
                net.BeginAcceptSocket(new AsyncCallback(OnConnectionAccepted), null);

            }
            net.Stop();
        }
        private static void OnConnectionAccepted(IAsyncResult ar)
        {
            TcpClient clientnet = net.EndAcceptTcpClient(ar);
            clients.Add(new Client(clientnet));
            Console.WriteLine("Client Connected");
        }
    }
}