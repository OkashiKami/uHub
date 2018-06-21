using System;
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
            while (true)
            {
                result = listener.BeginAcceptTcpClient(new AsyncCallback(OnClientAccepted), listener);
            }
        }

        private void OnClientAccepted(IAsyncResult ar)
        {
            Console.WriteLine("Client Connected");
            Client c = new Client(listener.EndAcceptTcpClient(ar));

        }
    }
}