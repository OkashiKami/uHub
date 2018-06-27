using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartDriver : MonoBehaviour {

    public float force;
	void FixedUpdate () {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.forward * force, ForceMode.Acceleration);
	}
}
