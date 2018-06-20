using System;
using System.Net.Sockets;
using System.Threading;

namespace Server02
{
    public class Client
    {
        private string id;
        private TcpClient clientnet;
        private byte[] readbuff;
        private Thread recievethread;
        private NetworkStream stream;

        public Client(TcpClient clientnet)
        {
            this.id = Guid.NewGuid().ToString().Split('-')[0];
            this.clientnet = clientnet;
            clientnet.SendBufferSize = 4096;
            clientnet.ReceiveBufferSize = 4096;
            stream = clientnet.GetStream();
            readbuff = new byte[4096];
            recievethread = new Thread(new ThreadStart(StartReciever));
            recievethread.Start();
        }

        private void StartReciever()
        {
            Console.WriteLine("Client {0} reciever started", id);
            while(clientnet.Connected)
            {
                byte[] buffer = new byte[clientnet.ReceiveBufferSize];
                stream.
            }
            Console.WriteLine("Client {0} reciever terminated", id);
        }
    }
}