using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager self;

    public ClientTCP client = new ClientTCP();
    public GameObject prefab;
    public Transform spawnpoint;
    public Dictionary<string, NetworkView> playerlist = new Dictionary<string, NetworkView>();
    public string id;
   
    private void Awake()
    {
        self = this;
        DontDestroyOnLoad(this.gameObject);
        ClientDataHandler.InitMessages();
        UThread.initUnityThread();
    }

    // Use this for initialization
    void Start () {
        client.Connect();
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
        playerlist.Add(id, view);
    }
    public void Destantiate(string id)
    {
        NetworkView view = null;
        playerlist.TryGetValue(id, out view);
        if(view) Destroy(view.gameObject);
    }

    public static void Send(byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
        buffer.WriteBytes(data);
        ClientTCP.self.mystream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
        buffer = null;
    }

    private void OnApplicationQuit()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteLong((long)PacketType._Leaving);
        buffer.WriteString(id);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor: Editor
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
        EditorGUILayout.LabelField("ID", nm.id);
        nm.client.IPADDRESS = EditorGUILayout.TextField("IP", nm.client.IPADDRESS);
        nm.client.PORT = EditorGUILayout.IntField("PORT", nm.client.PORT);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("Connection: " + nm.client.connecting);
        GUILayout.Box("Connected: " + nm.client.connected);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
        nm.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", nm.prefab, typeof(GameObject), true);
        nm.spawnpoint = (Transform)EditorGUILayout.ObjectField("Spawn", nm.spawnpoint, typeof(Transform), true);

        foreach(string s in nm.playerlist.Keys)
        {
            if (Selected == nm.playerlist[s]) GUI.color = Color.yellow;
            if(GUILayout.Button(nm.playerlist[s].gameObject.name))
            {
                if (Selected != nm.playerlist[s])
                    Selected = nm.playerlist[s];
                else Selected = null;
            }
            GUI.color = Color.white;
        }
    }
}
#endif
