﻿using System.Collections.Generic;
using UnityEngine;

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
        packets.Add((long)PacketType._TransformPosition, SP_TransformPosition);
        packets.Add((long)PacketType._TransformRotation, SP_TransformRotation);
        packets.Add((long)PacketType._GotTransformPosition, SP_GotTransformPosition);
        packets.Add((long)PacketType._GotTransformRotation, SP_GotTransformRotation);
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
        string id  = buffer.ReadString();
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
    private static void SP_TransformPosition(byte[] data)
    {
        long packetnum; ByteBuffer buffer;
        buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        packetnum = buffer.ReadLong();
        string id = buffer.ReadString();
        float x = buffer.ReadFloat();
        float y = buffer.ReadFloat();
        float z = buffer.ReadFloat();
        buffer.Dispose();
        NetworkView view = null;
        NetworkManager.self.playerlist.TryGetValue(id, out view);
        if(view != null && !view.isMine)
        {
            Debug.Log(string.Format("Changing {0} position", view.gameObject.name));
            view.RecievePosition(new Vector3(x, y, z));
        }
    }
    private static void SP_TransformRotation(byte[] data)
    {
        long packetnum; ByteBuffer buffer;
        buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        packetnum = buffer.ReadLong();
        string id = buffer.ReadString();
        float x = buffer.ReadFloat();
        float y = buffer.ReadFloat();
        float z = buffer.ReadFloat();
        buffer.Dispose();
        NetworkView view = null;
        NetworkManager.self.playerlist.TryGetValue(id, out view);
        if(view != null && !view.isMine) 
        {
            Debug.Log(string.Format("Changing {0} rotation", view.gameObject.name));
            view.RecieveRotation(Quaternion.Euler(x, y, z));
        }
    }

    private static void SP_GotTransformPosition(byte[] data)
    {
        long packetnum; ByteBuffer buffer;
        buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        packetnum = buffer.ReadLong();
        string id = buffer.ReadString();
        buffer.Dispose();
        NetworkView view = null;
        NetworkManager.self.playerlist.TryGetValue(id, out view);
        if (view != null && view.isMine)
        {
            Debug.Log(string.Format("Sending {0} position", view.gameObject.name));
            view.SendPosition();
        }
    }
    private static void SP_GotTransformRotation(byte[] data)
    {
        long packetnum; ByteBuffer buffer;
        buffer = new ByteBuffer();
        buffer.WriteBytes(data);
        packetnum = buffer.ReadLong();
        string id = buffer.ReadString();
        buffer.Dispose();
        NetworkView view = null;
        NetworkManager.self.playerlist.TryGetValue(id, out view);
        if (view != null && view.isMine)
        {
            Debug.Log(string.Format("Sending {0} rotation", view.gameObject.name));
            view.SendRotation();
        }
    }
}