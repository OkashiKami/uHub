using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkView : MonoBehaviour
{
    public bool isMine;
    public string id;

    List<Vector3> wantPositions = new List<Vector3>();
    List<Quaternion> wantRotations = new List<Quaternion>();
    public float closeEnoughThreshold = 0.01f;
    public float moveLerp = 0.8f;
    public float rotateLerp = 0.95f;

    private void Start()
    {
        if (isMine)
        {
            SendPosition();
            SendRotation();
        }
    }

    public void FixedUpdate()
    {
       
        if(!isMine)
        {
            ApplyPosition();
            ApplyRotation();
        }
    }

    public void SendPosition()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteLong((long)PacketType._TransformPosition);
        buffer.WriteString(id);
        float x = float.Parse(transform.position.x.ToString());
        float y = float.Parse(transform.position.y.ToString());
        float z = float.Parse(transform.position.z.ToString());
        buffer.WriteFloat(x);
        buffer.WriteFloat(y);
        buffer.WriteFloat(z);
        ClientTCP.self.Send(buffer.ToArray());
        buffer.Dispose();
    }
    public void SendRotation()
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteLong((long)PacketType._TransformRotation);
        buffer.WriteString(id);
        float x = float.Parse(transform.rotation.eulerAngles.x.ToString());
        float y = float.Parse(transform.rotation.eulerAngles.y.ToString());
        float z = float.Parse(transform.rotation.eulerAngles.z.ToString());
        buffer.WriteFloat(x);
        buffer.WriteFloat(y);
        buffer.WriteFloat(z);
        ClientTCP.self.Send(buffer.ToArray());
        buffer.Dispose();
    }

    private void ApplyPosition()
    {
        if (wantPositions.Count <= 0) return;
        if (Vector3.Distance(transform.position, wantPositions[0]) > closeEnoughThreshold)
        {
            transform.position = Vector3.Lerp(transform.position, wantPositions[0], moveLerp);
        }
        else wantPositions.RemoveAt(0);
    }
    public void ApplyRotation()
    {
        if (wantRotations.Count <= 0) return;
        if (Quaternion.Angle(transform.rotation, wantRotations[0]) > 5)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, wantRotations[0], rotateLerp);
        }
        else wantRotations.RemoveAt(0);

    }

    public void RecievePosition(Vector3 vector)
    {
        wantPositions.Add(vector);
    }
    public void RecieveRotation(Quaternion quaternion)
    {
        wantRotations.Add(quaternion);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NetworkView))]
public class NetworkViewEditor: Editor
{
    public override void OnInspectorGUI()
    {
        NetworkView view = (NetworkView)target;
        EditorGUILayout.LabelField("My ID", !string.IsNullOrEmpty(view.id) ? view.id : "Not Set");
        EditorGUILayout.LabelField("Is Mine", view.isMine.ToString());
        view.closeEnoughThreshold = EditorGUILayout.FloatField("CET", view.closeEnoughThreshold);
        view.moveLerp = EditorGUILayout.FloatField("Move Lerp", view.moveLerp);
        view.rotateLerp = EditorGUILayout.FloatField("Rotate Lerp", view.rotateLerp);
        Repaint();
    }
}
#endif