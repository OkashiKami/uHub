using System.Collections.Generic;
using UnityEngine;

namespace uHub.Utils
{
    using uHub.Networking;
    using NetworkView = uHub.Networking.NetworkView;

    class ClientDataHandler
    {
        public static ByteBuffer playerbuffer;
        public delegate void Packet_(byte[] data);
        private static Dictionary<long, Packet_> packets = new Dictionary<long, Packet_>();

        private static NetworkManager nm;
        private static long pLength;

        public static void InitMessages()
        {
            Debug.Log("Initializing Network Messages...");
            packets.Add((long)PacketType._Welcome, SP_Welcome);
            packets.Add((long)PacketType._JoinMap, SP_JoinMap);
            packets.Add((long)PacketType._LeaveMap, SP_LeaveMap);
            packets.Add((long)PacketType._NetView, SP_NetView);
        }

        public static void HandelData(byte[] data)
        {
            byte[] buffer;
            buffer = (byte[])data.Clone();

            if (playerbuffer == null) playerbuffer = new ByteBuffer();
            playerbuffer.WriteBytes(buffer);

            if (playerbuffer.Count() == 0)
            {
                playerbuffer.Clear();
                return;
            }

            if (playerbuffer.Length() >= 8)
            {
                pLength = playerbuffer.ReadLong(false);
                if (pLength <= 0)
                {
                    playerbuffer.Clear();
                    return;
                }
            }

            if (playerbuffer.Length() >= 8)
            {
                pLength = playerbuffer.ReadLong(false);
                if (pLength <= 0)
                {
                    playerbuffer.Clear();
                    return;
                }
            }

            while (pLength > 0 && pLength <= playerbuffer.Length() - 8)
            {
                if (pLength <= playerbuffer.Length() - 8)
                {
                    playerbuffer.ReadLong();
                    data = playerbuffer.ReadBytes((int)pLength);
                    HandelDataPacket(data);
                }
                pLength = 0;
                if (playerbuffer.Length() >= 8)
                {
                    pLength = playerbuffer.ReadLong(false);
                    if (pLength < 0)
                    {
                        playerbuffer.Clear();
                        return;
                    }
                }
            }
        }
        private static void HandelDataPacket(byte[] data)
        {
            long packetnum; ByteBuffer buffer; Packet_ packet;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            buffer = null;
            if (packetnum == 0) return;
            if (packets.TryGetValue(packetnum, out packet))
            {
                packet?.Invoke(data);
            }
        }

        private static void SP_Welcome(byte[] data)
        {
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string id = buffer.ReadString();
            Debug.Log(buffer.ReadString());
            NetworkManager.self.id = id;
            NetworkManager.self.Instantiate(id);
            buffer.Dispose();
        }
        private static void SP_JoinMap(byte[] data)
        {
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string id = buffer.ReadString();

            if (id == NetworkManager.self.id) return;


            NetworkManager.self.Instantiate(id, true);
            buffer.Dispose();
        }
        private static void SP_LeaveMap(byte[] data)
        {
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string id = buffer.ReadString();

            if (id == NetworkManager.self.id) return;

            NetworkManager.self.Destantiate(id);
            buffer.Dispose();
        }
        private static void SP_NetView(byte[] data)
        {
            long packetnum; ByteBuffer buffer;
            buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadLong();
            string id = buffer.ReadString();

            NetworkView view = null;
            NetworkManager.self.playerlist.TryGetValue(id, out view);
            if (view != null && !view.isMine)
            {
                //Position
                view.transform.position = new Vector3(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
                //Rotation
                view.transform.rotation = Quaternion.Euler(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
            }
            buffer.Dispose();
        }
    }
}