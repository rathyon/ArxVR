using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelControler : MonoBehaviour
{

    public GameObject impact;
    public GameObject fire;
    public GameObject doorpuzzle;

    public bool gazeOnOff = false;
    public bool imHit = false;
    public bool inDoorRange = false;
    // Use this for initialization
    void Start()
    {

    }

    public void Currentlygazing()
    {
        gazeOnOff = !gazeOnOff;
    }

    public void Spell()
    {
        //whatever the spell does
        if (imHit)
        {
            
        }
    }



    // Update is called once per frame
    void Update()
    {
        //can receive spell
        Spell();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fireball"))
        {
            imHit = true;
            fire.SetActive(true);
            impact.SetActive(true);
            gameObject.GetComponent<MeshFilter>().mesh.Clear();
            if (inDoorRange) {
                Destroy(doorpuzzle);
            }
        }

        if (other.CompareTag("DoorPuzzle1")) {
            inDoorRange = true;
        }

    }
}
