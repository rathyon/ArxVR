using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalActivat : MonoBehaviour {

    public GameObject cristal1;
    public GameObject cristal2;
    public GameObject cristal3;
    public GameObject cristal4;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (cristal1.GetComponent<CristalActivateFire>().activatedCristal && cristal2.GetComponent<CristalActivateAnimate>().activatedCristal
            && cristal3.GetComponent<CristalActivateWater>().activatedCristal && cristal4.GetComponent<CristalActivateLight>().activatedCristal) {
            gameObject.GetComponent<Outline>().enabled = true;
            gameObject.transform.Find("Portal").gameObject.SetActive(true);
        }
	}
}
