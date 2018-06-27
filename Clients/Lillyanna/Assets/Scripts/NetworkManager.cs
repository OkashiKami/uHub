using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uHub.Networking
{
    using System;
    using System.Globalization;
    using uHub.Utils;

    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager self;
        public GameObject prefab;
        public Transform spawnpoint;
        public Dictionary<string, NetworkView> playerlist = new Dictionary<string, NetworkView>();
        public string id;

        public string IPADDRESS = "127.0.0.1";
        public TcpClient socket;
        public bool connecting, connected;
        private byte[] buffer;
        public SocketError errorCode;
        public bool ConnectOnStart;
        private INIParser accountparser;

        private void Awake()
        {
            self = this;
            DontDestroyOnLoad(this.gameObject);
            UThread.initUnityThread();
            accountparser = new INIParser();
            string path = Application.dataPath.Replace("/", "\\").Replace("\\Assets", string.Empty);
            accountparser.Open(path + "\\account.dat", true);
            if (!accountparser.IsKeyExists("basic", "username"))
                accountparser.WriteValue("basic", "username", "admin");
            if (!accountparser.IsKeyExists("basic", "password"))
                accountparser.WriteValue("basic", "password", "admin");

            var date = DateTime.Now;
            var formattedDate = string.Format("{0}/{1}/{2}", date.Month, date.Day, date.Year);
            var time = new DateTime(DateTime.Now.TimeOfDay.Ticks); // Date part is 01-01-0001
            var formattedTime = formattedDate + "-" + time.ToString("h:mm:sstt", CultureInfo.InvariantCulture);
            accountparser.WriteValue("basic", "lastlogin", formattedTime);
        }

        // Use this for initialization
        void Start()
        {
            if(ConnectOnStart)
            Connect();
        }
        private void OnApplicationQuit()
        {
            CloseSocket();
        }

        public void Instantiate(string id, bool isRemote = false)
        {
            Vector3 spos;
            float x = new System.Random().Next(-5, 5);
            float z = new System.Random().Next(-5, 5);
            spos = new Vector3(spawnpoint.position.x + x, spawnpoint.position.y, spawnpoint.position.z + z);

            GameObject tmp = Instantiate(prefab, spos, Quaternion.identity);
            tmp.name = string.Format("Player {0} [{1}]", id, isRemote ? "REMOTE" : "SELF");
            NetworkView view = tmp.GetComponent<NetworkView>();
            view.SetId(id);
            view.SetIsMine(!isRemote);
            if (!view.isMine) Destroy(tmp.GetComponent<Orbit>());
            playerlist.Add(id, view);
        }
        public void Destantiate(string id)
        {
            NetworkView view = null;
            playerlist.TryGetValue(id, out view);
            if (view) Destroy(view.gameObject);
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
        }
        private void OnReceive(IAsyncResult ar)
        {
            int numberofbytes = socket.Client.EndReceive(ar);
            Packet p = new Packet().Deserialize(buffer);
            if (numberofbytes <= 0) { }
            else
            {
                var cmd = p.ReadString();
                var id = p.ReadString();
                var msg = p.ReadString();

                switch(cmd)
                {
                    case "-welcome":
                        this.id = id;
                        break;
                    case "-welcomeremote":
                        if(this.id != id)
                        {

                        }
                        break;
                    case "-netviewupdate":
                        if (this.id != id)
                        {

                        }
                        break;
                    case "-remoteleft":
                        if(this.id != id)
                        {

                        }
                        break;
                }

                Debug.Log(string.Format("Recieved {0} from {1}", msg, id));

                p.Dispose();
            }
            socket.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, out errorCode, new AsyncCallback(OnReceive), socket);
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
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkManager))]
    public class NetworkManagerEditor : Editor
    {
        public NetworkView Selected { get; private set; }

        public void OnSceneGUI()
        {
            if (Selected != null)
            {
                Handles.color = new Color32(255, 0, 0, 50);
                Handles.CubeHandleCap(0, Selected.transform.position, Quaternion.identity, 1F, EventType.Repaint);
            }
        }

        public override void OnInspectorGUI()
        {
            NetworkManager nm = (NetworkManager)target;
            //base.OnInspectorGUI();
            GUI.skin = Resources.Load<GUISkin>("eskin");
            GUILayout.Box("Network");
            nm.ConnectOnStart = EditorGUILayout.Toggle("Connect On Start", nm.ConnectOnStart);
            EditorGUILayout.LabelField("ID", nm.id);
            nm.IPADDRESS = EditorGUILayout.TextField("IP", nm.IPADDRESS);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Box("Connection: " + nm.connecting);
            GUILayout.Box("Connected: " + nm.connected);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            nm.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", nm.prefab, typeof(GameObject), true);
            nm.spawnpoint = (Transform)EditorGUILayout.ObjectField("Spawn", nm.spawnpoint, typeof(Transform), true);

            foreach (string s in nm.playerlist.Keys)
            {
                if (Selected == nm.playerlist[s]) GUI.color = Color.yellow;
                if (GUILayout.Button(nm.playerlist[s].gameObject.name))
                {
                    if (Selected != nm.playerlist[s])
                        Selected = nm.playerlist[s];
                    else Selected = null;
                }
                GUI.color = Color.white;
            }
            if(nm.errorCode != SocketError.NoData)
            {
                EditorGUILayout.HelpBox(nm.errorCode.ToString(), MessageType.Info);
            }
        }
    }
#endif
}