using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellbookController : MonoBehaviour {

    public GameObject spellbook;

    private Quaternion orig;

	// Use this for initialization
	void Start () {
        spellbook.SetActive(false);
        orig = spellbook.transform.rotation;
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            spellbook.SetActive(!spellbook.activeSelf);
        }

        if (spellbook.activeSelf)
        {
            spellbook.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f + Camera.main.transform.right * 0.82f + Camera.main.transform.up * (-0.413f);
            spellbook.transform.rotation = Camera.main.transform.rotation * orig;
        }
    }
}
