using System;
using System.Net.Sockets;

namespace uHub.Entities
{
    using uHub.Databse;
    using uHub.Utils;

    public class Client
    {
        public string id;
        public string ip;
        public TcpClient clientSocket;
        public NetworkStream mystream;
        private byte[] readbuff;

        public Inventory inventory;

        public Client(TcpClient clientSocket)
        {
            id = Guid.NewGuid().ToString().Split('-')[0];
            this.clientSocket = clientSocket;
            ip = this.clientSocket.Client.RemoteEndPoint.ToString();
            inventory = new Inventory();
            Start();
        }
        public void Start()
        {
            clientSocket.SendBufferSize = Constants.MAX_BUFFER_SIZE;
            clientSocket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
            mystream = clientSocket.GetStream();
            readbuff = new byte[9792];
            mystream.BeginRead(readbuff, 0, clientSocket.ReceiveBufferSize, new AsyncCallback(OnRecieveData), clientSocket);
        }
        private void OnRecieveData(IAsyncResult ar)
        {
            try
            {
                int readbytes = mystream.EndRead(ar);
                if (readbytes > 0)
                {
                    byte[] newbytes = new byte[readbytes];
                    Buffer.BlockCopy(readbuff, 0, newbytes, 0, readbytes);
                    ServerDataHandler.HandelData(id, newbytes);
                }
                try
                {
                    mystream.BeginRead(readbuff, 0, Constants.MAX_BUFFER_SIZE, new AsyncCallback(OnRecieveData), clientSocket);
                }
                catch(Exception ex)
                {
                    Program.Log("Error: {0}", ex);
                    CloseSocket();
                    return;
                }
            }
            catch (Exception ex)
            {
                Program.Log("Error: {0}", ex);
                CloseSocket();
                return;
            }
        }
        public void CloseSocket()
        {
            Program.Log("Connection from {0} has been terminated!", ip);
            clientSocket.Close();
            ServerTCP.RemoveClient(id);
        }
    }
}