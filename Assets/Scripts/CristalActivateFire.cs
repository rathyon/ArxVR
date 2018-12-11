using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CristalActivateFire : MonoBehaviour
{

    public GameObject light;
    public bool activatedCristal = false;
    // Use this for initialization
    void Start()
    {

    }

    public void activateCristal()
    {
        activatedCristal = true;
        light.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
