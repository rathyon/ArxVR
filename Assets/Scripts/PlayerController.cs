using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float h = Input.GetAxis("Horizontal") * 0.1f;
        float v = Input.GetAxis("Vertical") * 0.1f;

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        Vector3 right = Camera.main.transform.right;
        right.y = 0;

        Vector3 delta = forward * v + right * h;

        transform.position += delta;
    }
}
