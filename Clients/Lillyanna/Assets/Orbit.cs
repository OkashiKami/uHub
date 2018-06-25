using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {

    float time = 0, wantTime = 0.05f;
    public bool Negitive;
    void Update()
    {
        time += 1 * Time.deltaTime;
        if (time >= wantTime)
        {
            transform.RotateAround(Vector3.zero, Vector3.up, Negitive ? -10 : 10);
            time = 0;
        }
        transform.LookAt(Vector3.zero);
    }
}
