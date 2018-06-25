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
        public TcpClient socket;
        public bool connecting, connected;
        private byte[] buffer;
        public SocketError errorCode;

        public ClientTCP()
        {
            self = this;
        }

        public void Connect()
        {
            socket = new TcpClient();
            socket.ReceiveBufferSize = Constants.MAX_BUFFER_SIZE;
            socket.SendBufferSize = Constants.MAX_BUFFER_SIZE;
            socket.NoDelay = false;
            buffer = new byte[Constants.MAX_BUFFER_SIZE];
            socket.BeginConnect(IPADDRESS, Constants.DEFAULT_PORT, new AsyncCallback(ConnectCallback), socket);
            connecting = true;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                socket.EndConnect(ar);
                if (!socket.Connected)
                {
                    connected = false;
                    connecting = false;
                    return;
                }
                else
                {
                    socket.NoDelay = true;
                    connected = true;
                    connecting = false;
                    Debug.Log("Successfully connected to server");
                    socket.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out errorCode, new AsyncCallback(OnReceive), socket);
                }
            }
            catch (Exception)
            {
                connecting = false;
                connected = false;
                Debug.Log("Unable to connect to server");

            }
        }


        public void Send(byte[] buffer)
        {
            socket.Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnSend), socket);
        }
        public void Send(Packet packet)
        {
            var buffer = packet.Serialize();
            socket.Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnSend), socket);
        }

        private void OnSend(IAsyncResult ar)
        {
            int numberofbytes = socket.Client.EndSend(ar);
            Debug.Log(string.Format("{0} of bytes where sent", numberofbytes));
        }
        private void OnReceive(IAsyncResult ar)
        {
            int numberofbytes = socket.Client.EndReceive(ar);
            Packet p = new Packet().Deserialize(buffer);
            if (numberofbytes <= 0) { Debug.Log("Not enough bytes read"); }
            else
            {
                var cmd = p.ReadString();

                if (cmd == "-welcome")
                {
                    var id = p.ReadString();
                    var msg = p.ReadString();
                    UThread.executeInUpdate(() => { NetworkManager.self.Instantiate(id); });
                    if(!string.IsNullOrEmpty(msg)) Debug.Log("Recieved: " + msg);
                }
                else if (cmd == "-welcomeremote")
                {
                    var id = p.ReadString();
                    var msg = p.ReadString();
                    UThread.executeInUpdate(() => { NetworkManager.self.Instantiate(id, true); });
                    if (!string.IsNullOrEmpty(msg)) Debug.Log("Recieved: " + msg);
                }
                else if(cmd == "-netviewupdate")
                {
                    UThread.executeInUpdate(() =>
                    {
                        foreach(NetworkView nv in UnityEngine.Object.FindObjectsOfType<NetworkView>())
                        {
                            if(nv.id == p.ReadString() && !nv.isMine)
                            {
                                Debug.Log("Networkview has been found");
                                nv.Apply(p.ReadFloat(), p.ReadFloat(), p.ReadFloat(), p.ReadFloat(), p.ReadFloat(), p.ReadFloat());
                            }
                        }
                    });
                }
                else if(cmd == "-remoteleft")
                {
                    var id = p.ReadString();
                    UThread.executeInUpdate(() => 
                    {
                        NetworkView[] nvs = UnityEngine.Object.FindObjectsOfType<NetworkView>();
                        foreach (NetworkView nv in nvs)
                        {
                            if (nv.id == id)
                                UnityEngine.Object.DestroyImmediate(nv.gameObject);
                        }
                    });
                }

                if(!string.IsNullOrEmpty(cmd)) Debug.Log("Recieved: " + cmd);
                p.Dispose();
                socket.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out errorCode, new AsyncCallback(OnReceive), socket);
            }
        }

        public void CloseSocket()
        {
            Packet p = new Packet();
            p.WriteString("-closing");
            p.WriteString(NetworkManager.self.id);
            p.WriteString("Leaving, logging off...");
            
            Send(p.Serialize());
            p.Dispose();
            Debug.Log("Connection from server has been terminated!");
            socket.Close();
        }
    }

}