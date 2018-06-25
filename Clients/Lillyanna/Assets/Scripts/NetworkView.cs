using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uHub.Networking
{
    public class NetworkView : MonoBehaviour
    {
        public bool isMine { get; private set; }
        public string id { get; private set; }

        public void SetId(string value) { id = value; }
        public void SetIsMine(bool value) { isMine = value; }

        private List<Vector3> wantPositions = new List<Vector3>();
        private List<Quaternion> wantRotations = new List<Quaternion>();


        public enum UpdateType { Update, FixedUpdate, LateUpdate, TimedUpdate, TimedFixedUpdate, None}
        public UpdateType utype;

        public float time = 0, wantTime = 1;

        private void Start()
        {

        }

        private void Update()
        {
            if (isMine && utype == UpdateType.Update)
            {
                Send();
            }

            if(utype == UpdateType.TimedUpdate)
            {
                time += Time.deltaTime;
                if(time >= wantTime)
                {
                    Send();
                    time = 0;
                }
            }

        }
        public void FixedUpdate()
        {
            if (isMine && utype == UpdateType.FixedUpdate)
            {
                Send();
            }
            if (utype == UpdateType.TimedFixedUpdate)
            {
                time += Time.fixedDeltaTime;
                if (time >= wantTime)
                {
                    Send();
                    time = 0;
                }
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
            Packet p = new Packet();
            p.WriteString("-NetviewUpdate");
            p.WriteString(NetworkManager.self.id);
            p.WriteFloat(transform.position.x);
            p.WriteFloat(transform.position.y);
            p.WriteFloat(transform.position.z); 
            p.WriteFloat(transform.rotation.eulerAngles.x);
            p.WriteFloat(transform.rotation.eulerAngles.y);
            p.WriteFloat(transform.rotation.eulerAngles.z);

            NetworkManager.self.client.Send(p.Serialize());
        }
        public void Apply(float x1, float y1, float z1, float x2, float y2, float z2)
        {

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkView))]
    public class NetworkViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.skin = Resources.Load<GUISkin>("eskin");
            Repaint();
            
            NetworkView view = (NetworkView)target;
            view.utype = (NetworkView.UpdateType)EditorGUILayout.EnumPopup("Update Mode", view.utype);
            EditorGUILayout.LabelField("My ID", !string.IsNullOrEmpty(view.id) ? view.id : "Not Set");
            EditorGUILayout.LabelField("Is Mine", view.isMine.ToString());
            if (view.utype == NetworkView.UpdateType.TimedUpdate || view.utype == NetworkView.UpdateType.TimedFixedUpdate)
            {
                view.wantTime = EditorGUILayout.FloatField("Want Time", view.wantTime);
                var percentage = (view.time / view.wantTime) * 100;
                EditorGUILayout.Slider("%", percentage, 0, 100);
            }   
            Repaint();
        }
    }
#endif
}