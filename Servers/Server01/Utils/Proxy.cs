using System;

namespace uHub.Utils
{
    using System.Collections.Generic;
    using uHub.Entities;

    public class Proxy
    {
        // Functions Thar Are Recieved
        public static void RecieveNetworkView(byte[] data)
        {
            Program.Log("Someone networkview was updated...");
            for (int i = 0; i < ServerTCP.clients.Count; i++)
            {
                Client client = ServerTCP.clients[i];

                long packetnum; ByteBuffer buffer;
                buffer = new ByteBuffer();
                buffer.WriteBytes(data);
                packetnum = buffer.ReadLong();
                string myid = buffer.ReadString();
                float x = buffer.ReadFloat();
                float y = buffer.ReadFloat();
                float z = buffer.ReadFloat();
                Program.Log(string.Format("Client {0} sent {1}", myid, Enum.GetName(typeof(PacketType), packetnum)));
                client.SendData(data: buffer.ToArray());
                buffer.Dispose();
            }
        }
        public static void RecieveLeaving(byte[] data)
        {
            Program.Log("Someone is leaving...");
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string myid = buffer.ReadString();

            for (int i = 0; i < ServerTCP.clients.Count; i++)
            {
                if(ServerTCP.clients[i].id == myid)
                {
                    ServerTCP.clients[i].Close();
                }
            }
            
            buffer.Dispose();
        }
    }
}
