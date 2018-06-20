using System;
using System.Net.Sockets;
using UnityEngine;

[Serializable]
public class Player : MonoBehaviour
{
    public Health HealthModule = new Health();

    private void Awake()
    {
        HealthModule.Init(this);
    }
    void Update () {
        HealthModule.Update();
	}
}
