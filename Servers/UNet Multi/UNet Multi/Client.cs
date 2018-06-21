using System;
using System.Net.Sockets;

namespace UNetMulti
{
    internal class Client
    {
        public TcpClient client;
        private NetworkStream stream;
        private byte[] asyncbuff;
        public string id;
        public string ip;

        public Client(TcpClient tcpClient)
        {
            client = tcpClient;
            id = Guid.NewGuid().ToString().Split('-')[0];
            ip = client.Client.RemoteEndPoint.ToString();
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;
            client.NoDelay = false;
            asyncbuff = new byte[4096];
            stream = client.GetStream();
            stream.BeginRead(asyncbuff, 0, 4096, new AsyncCallback(OnRecieve), null);
        }

        public void Send(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
            buffer.WriteBytes(data);
            if (client != null && client.Connected)
                stream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            buffer.Dispose();
        }


        private void OnRecieve(IAsyncResult ar)
        {
            try
            {
                int readbytes = stream.EndRead(ar);
                if (readbytes > 0)
                {
                    byte[] newbytes = new byte[readbytes];
                    Buffer.BlockCopy(asyncbuff, 0, newbytes, 0, readbytes);
                    ServerDataHandler.HandelData(id, newbytes);
                    stream.BeginRead(asyncbuff, 0, 4096, new AsyncCallback(OnRecieve), null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
                CloseSocket();
                return;
            }
        }

        public void CloseSocket()
        {
            Console.WriteLine("Connection from {0} has been terminated!", ip);
            client.Close();
            Server.RemoveClient(this);
        }
    }
}