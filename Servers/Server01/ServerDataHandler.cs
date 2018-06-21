using System;
using System.Collections.Generic;
using System.Linq;
using uHub.Networking;

namespace uHub.Utils
{
    public class ServerDataHandler
    {
        private delegate void Packet_(string id, byte[] data);
        private static Dictionary<long, Packet_> packets = new Dictionary<long, Packet_>();
        public static long pLength;

        public static void InitMessages()
        {
            Program.Log("Initializing Network Messages...");
            packets.Add((long)PacketType._NetView, CP_NetView);
            packets.Add((long)PacketType._Leaving, CP_Leaving);
        }

        public static void HandelData(string id , byte[] data)
        {
            byte[] buffer;
            buffer = (byte[])data.Clone();

            if (Types.tmpPlayers.Count < Types.tmpPlayers.Capacity)
            {
                Types.TempPlayer tmp = new Types.TempPlayer();
                Types.tmpPlayers.Add(tmp);
                tmp.id = id;
                if (tmp.buffer == null) tmp.buffer = new ByteBuffer();
                tmp.buffer.WriteBytes(buffer);

                if (tmp.buffer.Count() == 0)
                {
                    tmp.buffer.Clear();
                    return;
                }
                if(tmp.buffer.Length() >= 4)
                {
                    pLength = tmp.buffer.ReadLong(false);
                    if(pLength <= 0)
                    {
                        tmp.buffer.Clear();
                        return;
                    }
                }

                while (pLength > 0 && pLength <= tmp.buffer.Length() - 8)
                {
                    if (pLength <= tmp.buffer.Length() - 8)
                    {
                        tmp.buffer.ReadLong();
                        data = tmp.buffer.ReadBytes((int)pLength);
                        HandelDataPacket(tmp.id, data);
                    }
                    pLength = 0;
                    if (tmp.buffer.Length() >= 4)
                    {
                        pLength = tmp.buffer.ReadLong(false);
                        if (pLength < 0)
                        {
                            tmp.buffer.Clear();
                            return;
                        }
                    }
                }
                if(pLength <= 1)
                {
                    tmp.buffer.Clear();
                    return;
                }
            }
        }

        private static void HandelDataPacket(string id, byte[] data)
        {
            long packetnum; ByteBuffer buffer; Packet_ packet;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            buffer = null;
            if (packetnum == 0)
            {
                Program.Log("No Packet Recieved");
                return;
            }
            else
            {
                if (packets.TryGetValue(packetnum, out packet))
                {
                    packet?.Invoke(id, data);
                }
            }
        }
        private static void CP_NetView(string id, byte[] data)
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
            ServerTCP.Send(buffer.ToArray());
            buffer.Dispose();
        }
        private static void CP_Leaving(string id, byte[] data)
        {
            Program.Log("Someone is leaving...");
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string myid = buffer.ReadString();
            ServerTCP.RemoveClient(myid);
            buffer.Dispose();
        }
    }
}
