using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

[Serializable]
public class ClientTCP
{
    public static ClientTCP self;
    public string IPADDRESS = "127.0.0.1";
    public int PORT = 2652;
    public TcpClient _clientSocket;
    public bool connecting, connected;
    public NetworkStream mystream;
    private byte[] asyncbuff;

    public ClientTCP()
    {
        self = this;
    }

    public void Connect()
    {
        _clientSocket = new TcpClient();
        _clientSocket.ReceiveBufferSize = 4096;
        _clientSocket.SendBufferSize = 4096;
        _clientSocket.NoDelay = false;
        asyncbuff = new byte[8192];
        _clientSocket.BeginConnect(IPADDRESS, PORT, new AsyncCallback(ConnectCallback), _clientSocket);
        connecting = true;
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            _clientSocket.EndConnect(ar);
            if(!_clientSocket.Connected)
            {
                connected = false;
                connecting = false;
                return;
            }
            else
            {
                _clientSocket.NoDelay = true;
                mystream = _clientSocket.GetStream();
                mystream.BeginRead(asyncbuff, 0, 8192, new AsyncCallback(OnRecieve), null);
                connected = true;
                connecting = false;
                Debug.Log("Successfully connected to server");
            }
        }
        catch (Exception)
        {
            connecting = false;
            connected = false;
            Debug.Log("Unable to connect to server");

        }
    }
    private void OnRecieve(IAsyncResult ar)
    {
        try
        {
            int byteAmt = mystream.EndRead(ar);
            byte[] mybytes = new byte[byteAmt];
            Buffer.BlockCopy(asyncbuff, 0, mybytes, 0, byteAmt);
            if (byteAmt == 0) return;

            UThread.executeInUpdate(()=> 
            {
                ClientDataHandler.HandelData(mybytes);
            });
            mystream.BeginRead(asyncbuff, 0, 8192, new AsyncCallback(OnRecieve), null);
        }
        catch (Exception)
        {
            
        }
    }
    public void Send(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
        buffer.WriteBytes(data);
        if (_clientSocket != null && _clientSocket.Connected)
            mystream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
        buffer.Dispose();
    }
}
