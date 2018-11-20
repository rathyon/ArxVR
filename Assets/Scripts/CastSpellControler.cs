using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastSpellControler : MonoBehaviour {

    public GameObject spell;
    private Ray direction;
    private RaycastHit hit;

    public float speed = 0.2f;
	// Use this for initialization
	void Start () {
        Cursor.visible = false;
	}

	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.JoystickButton2) ||Input.GetKeyDown("a")){

            //Ray dir = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            GameObject clone = Instantiate(spell, transform.position, transform.rotation);
            clone.GetComponent<SpellControler>().direction = Camera.main.transform.forward; 

        }

	}
}
