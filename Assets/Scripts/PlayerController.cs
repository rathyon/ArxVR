using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    bool can_move;

	// Use this for initialization
	void Start () {
        can_move = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (can_move)
                can_move = false;
            else
                can_move = true;
        }

        if (can_move)
        {
            float h = Input.GetAxis("Horizontal") * 0.05f;
            float v = Input.GetAxis("Vertical") * 0.05f;

            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            Vector3 right = Camera.main.transform.right;
            right.y = 0;

            Vector3 delta = forward * v + right * h;

            transform.position += delta;
        }
    }
}
