using System;
using System.Net.Sockets;

namespace uHub.Entities
{
    using uHub.Databse;
    using uHub.Databse.Entities;
    using uHub.Utils;

    public class Client
    {
        public string id;
        public string ip;
        public TcpClient socket;
        public NetworkStream mystream;
        private byte[] readbuff;
        public bool isClosed;
        public Entity entity;
        public Inventory inventory;

        public Client(TcpClient socket)
        {
            isClosed = false;
            id = Guid.NewGuid().ToString().Split('-')[0];
            this.socket = socket;
            ip = this.socket.Client.RemoteEndPoint.ToString();
            inventory = new Inventory();
            entity = EntityDatabase.player;
            Open();
        }
        public void Open()
        {
            isClosed = false;
            Program.Log("Connection recieved from {0}", ip);
            socket.SendBufferSize = Constants.MAX_BUFFER_SIZE;
            socket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
            mystream = socket.GetStream();
            readbuff = new byte[9792];
            mystream.BeginRead(readbuff, 0, Constants.MAX_BUFFER_SIZE, new AsyncCallback(OnRecieveData), socket);
            SendWelcomeMessage();
        }
        public void Close()
        {
            SendLeaveMap();
            socket.Close();
            isClosed = true;
            Program.Log("Connection from {0} has been terminated!", ip);
        }

        public void SendData(byte[] data = default)
        {
            if (data != null)
            {
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
                buffer.WriteBytes(data);
                if (socket != null && socket.Connected)
                    mystream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                buffer.Dispose();
            }
            else Program.Log("Can't send blank data, either the id or the data is empty");
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
                    mystream.BeginRead(readbuff, 0, Constants.MAX_BUFFER_SIZE, new AsyncCallback(OnRecieveData), socket);
                }
                catch(Exception ex)
                {
                    Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                Program.Log("Error: {0}", ex);
                Close();
                return;
            }
        }


        //Pre Messagers
        public void SendWelcomeMessage()
        {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)PacketType._Welcome);
            buffer.WriteString(id);
            buffer.WriteString(string.Format("[{0}] Welcome To the server...", DateTime.Now.ToShortTimeString()));
            SendData(buffer.ToArray());
            SendJoinMap();
            buffer.Dispose();
        }
        public void SendJoinMap()
        {
            foreach(Client c in ServerTCP.clients)
            {
                if(c.id != id)
                {
                    ByteBuffer buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._JoinMap);
                    buffer.WriteString(id);
                    c.SendData(buffer.ToArray());
                    buffer.Dispose();
                }
            }
        }
        public void SendLeaveMap()
        {
            foreach(Client c in ServerTCP.clients)
            {
                if (c.id != id)
                {
                    ByteBuffer buffer = new ByteBuffer();
                    buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._LeaveMap);
                    buffer.WriteString(id);
                    c.SendData(buffer.ToArray());
                    buffer.Dispose();
                }
            }
        }
    }
}