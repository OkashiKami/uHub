using System;
using System.Net.Sockets;
using UnityEngine;

namespace uHub.Networking
{
    using uHub.Utils;

    [Serializable]
    public class ClientTCP
    {
        public static ClientTCP self;
        public string IPADDRESS = "127.0.0.1";
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
            _clientSocket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
            _clientSocket.SendBufferSize = Constants.MAX_BUFFER_SIZE;
            _clientSocket.NoDelay = false;
            asyncbuff = new byte[Constants.MAX_BUFFER_SIZE];
            _clientSocket.BeginConnect(IPADDRESS, Constants.DEFAULT_PORT, new AsyncCallback(ConnectCallback), _clientSocket);
            connecting = true;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndConnect(ar);
                if (!_clientSocket.Connected)
                {
                    connected = false;
                    connecting = false;
                    return;
                }
                else
                {
                    _clientSocket.NoDelay = true;
                    mystream = _clientSocket.GetStream();
                    connected = true;
                    connecting = false;
                    Debug.Log("Successfully connected to server");
                    mystream.BeginRead(asyncbuff, 0, Constants.MAX_BUFFER_SIZE, new AsyncCallback(OnRecieve), null);
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
                int readbytes = mystream.EndRead(ar);
                if (readbytes > 0)
                {
                    byte[] newbytes = new byte[readbytes];
                    Buffer.BlockCopy(asyncbuff, 0, newbytes, 0, readbytes);
                    UThread.executeInUpdate(() =>
                    {
                        ClientDataHandler.HandelData(newbytes);
                    });
                }
                mystream.BeginRead(asyncbuff, 0, Constants.MAX_BUFFER_SIZE, new AsyncCallback(OnRecieve), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
                CloseSocket();
                return;
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

        public void CloseSocket()
        {
            Debug.Log("Connection from server has been terminated!");
            _clientSocket.Close();
        }
    }

}