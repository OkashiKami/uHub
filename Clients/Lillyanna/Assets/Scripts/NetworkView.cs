using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkView : MonoBehaviour
{
    public bool isMine { get; private set; }
    public string id { get; private set; }

    public void SetId(string value) { id = value; }
    public void SetIsMine (bool value) { isMine = value; }

    private List<Vector3> wantPositions = new List<Vector3>();
    private List<Quaternion> wantRotations = new List<Quaternion>();
    

    public enum UpdateType { Update, FixedUpdate, LateUpdate }
    public UpdateType utype;

    private void Start()
    {
       
    }

    private void Update()
    {
        if (isMine && utype == UpdateType.Update)
        {
            Send();
        }
    }
    public void FixedUpdate()
    {
        if (isMine && utype == UpdateType.FixedUpdate)
        {
            Send();
        }
    }
    private void LateUpdate()
    {
        if (isMine && utype == UpdateType.LateUpdate)
        {
            Send();
        }
    }

    public void Send()
    {
        ByteBuffer buffer;

        buffer = new ByteBuffer();
        buffer.WriteLong((long)PacketType._NetView);
        buffer.WriteString(id);

        //Position
        buffer.WriteFloat(float.Parse(transform.position.x.ToString()));
        buffer.WriteFloat(float.Parse(transform.position.y.ToString()));
        buffer.WriteFloat(float.Parse(transform.position.z.ToString()));

        //Rotation
        buffer.WriteFloat(float.Parse(transform.rotation.eulerAngles.x.ToString()));
        buffer.WriteFloat(float.Parse(transform.rotation.eulerAngles.y.ToString()));
        buffer.WriteFloat(float.Parse(transform.rotation.eulerAngles.z.ToString()));
        
        ClientTCP.self.Send(buffer.ToArray());
        buffer.Dispose();
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(NetworkView))]
public class NetworkViewEditor: Editor
{
    public override void OnInspectorGUI()
    {
        NetworkView view = (NetworkView)target;
        view.utype = (NetworkView.UpdateType)EditorGUILayout.EnumPopup("Update Mode", view.utype);
        EditorGUILayout.LabelField("My ID", !string.IsNullOrEmpty(view.id) ? view.id : "Not Set");
        EditorGUILayout.LabelField("Is Mine", view.isMine.ToString());
        Repaint();
    }
}
#endif