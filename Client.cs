using System;
using System.Net.Sockets;

namespace uHub
{
    public class Client
    {
        public string id;
        public string ip;
        public TcpClient clientSocket;
        public NetworkStream mystream;
        private byte[] readbuff;

        public Client(TcpClient clientSocket)
        {
            id = Guid.NewGuid().ToString().Split('-')[0];
            this.clientSocket = clientSocket;
            this.ip = this.clientSocket.Client.RemoteEndPoint.ToString();
            Start();
        }
        public void Start()
        {
            clientSocket.SendBufferSize = 4096;
            clientSocket.ReceiveBufferSize = 4096;
            mystream = clientSocket.GetStream();
            readbuff = new byte[4096];
            mystream.BeginRead(readbuff, 0, clientSocket.ReceiveBufferSize, new AsyncCallback(OnRecieveData), clientSocket);
        }
        private void OnRecieveData(IAsyncResult ar)
        {
            try
            {
                int readbytes = mystream.EndRead(ar);
                if(readbytes <= 0)
                {
                    CloseSocket();
                    return;
                }
                byte[] newbytes = new byte[readbytes];
                Buffer.BlockCopy(readbuff, 0, newbytes, 0, readbytes);
                ServerDataHandler.HandelData(id, newbytes);
                mystream.BeginRead(readbuff, 0, clientSocket.ReceiveBufferSize, new AsyncCallback(OnRecieveData), clientSocket);
            }
            catch (Exception)
            {
                CloseSocket();
                return;
            }
        }
        public void Send(byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
            buffer.WriteBytes(data);
            if(clientSocket != null && clientSocket.Connected)
                mystream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            buffer.Dispose();
        }
        private void CloseSocket()
        {
            Program.Log("Connection from {0} has been terminated!", ip);
            clientSocket.Close();
            ServerTCP.RemoveClient(this);
        }
    }
}