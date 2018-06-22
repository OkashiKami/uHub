using System;

namespace uHub.Utils
{
    using uHub.Entities;
    using uHub.Networking;

    public class Proxy
    {
        // General Send Function
        public static void Send(string id = default(string), byte[] data = default(byte[]))
        {
            if (id == null && data != null)
            {
                for (int i = 0; i < ServerTCP.clients.Count; i++)
                {
                    Client c = ServerTCP.clients[i];
                    ByteBuffer buffer = new ByteBuffer();
                    buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
                    buffer.WriteBytes(data);
                    if (c.clientSocket != null && c.clientSocket.Connected)
                        c.mystream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                    buffer.Dispose();
                }
            }
            else if (id != null && data != null)
            {
                for (int i = 0; i < ServerTCP.clients.Count; i++)
                {
                    if (ServerTCP.clients[i].id == id)
                    {
                        Client c = ServerTCP.clients[i];
                        ByteBuffer buffer = new ByteBuffer();
                        buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
                        buffer.WriteBytes(data);
                        if (c.clientSocket != null && c.clientSocket.Connected)
                            c.mystream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                        buffer.Dispose();
                    }
                }
            }
            else Program.Log("Can't send blank data, either the id or the data is empty");
        }

        // Functions To Send
        public static void SendJoinMap(string id)
        {
            for (int i = 0; i < ServerTCP.clients.Count; i++)
            {
                if (ServerTCP.clients[i].clientSocket != null && ServerTCP.clients[i].id != id)
                {
                    ByteBuffer buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._JoinMap);
                    buffer.WriteString(ServerTCP.clients[i].id);
                    Send(id, buffer.ToArray());
                    buffer.Dispose();

                    buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._JoinMap);
                    buffer.WriteString(id);
                    Send(ServerTCP.clients[i].id, buffer.ToArray());
                    buffer.Dispose();
                }
            }
        }
        public static void SendLeaveMap(string id)
        {
            for (int i = 0; i < ServerTCP.clients.Count; i++)
            {
                if (ServerTCP.clients[i].clientSocket != null && ServerTCP.clients[i].id != id)
                {
                    ByteBuffer buffer = new ByteBuffer();
                    buffer = new ByteBuffer();
                    buffer.WriteLong((long)PacketType._LeaveMap);
                    buffer.WriteString(id);
                    Send(ServerTCP.clients[i].id, buffer.ToArray());
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
        
        // Functions Thar Are Recieved
        public static void RecieveNetworkView(string id, byte[] data)
        {
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string myid = buffer.ReadString();
            float x = buffer.ReadFloat();
            float y = buffer.ReadFloat();
            float z = buffer.ReadFloat();
            Program.Log(string.Format("Client {0} sent {1}", myid, Enum.GetName(typeof(PacketType), packetnum)));
            Proxy.Send(data: buffer.ToArray());
            buffer.Dispose();
        }
        public static void RecieveLeaving(string id, byte[] data)
        {
            Program.Log("Someone is leaving...");
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string myid = buffer.ReadString();
            ServerTCP.RemoveClient(myid);
            Proxy.SendLeaveMap(myid);
            buffer.Dispose();
        }
    }
}
