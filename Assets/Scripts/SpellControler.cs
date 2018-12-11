using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellControler : MonoBehaviour {

    public Vector3 direction;
    public GameObject impactPS;
    //public GameObject reticleDirection;
    public float time = 0;
    public float timeDead = 0;
    public bool impact;

	// Use this for initialization
	void Start () {
        impact = false;
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        
        Debug.Log(transform.position);
        if(time >= 15.0){
            Destroy(gameObject);
        }
        
        if (impact) {
            timeDead += Time.deltaTime;
            Debug.Log(transform.position);
            if (timeDead >= 1.0){
                Destroy(gameObject);
            }
        }
        else {
                transform.position += direction * 0.1f;
        }
        
	}

	private void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Player") || (other.CompareTag("TorchLighting")))
        {
            
        }
        else if (other.CompareTag("Barrel"))
        {

            GetComponent<MeshFilter>().mesh.Clear();
            impactPS.SetActive(true);
            impact = true;
        }
        else {
            Debug.Log(other.tag);
            GetComponent<MeshFilter>().mesh.Clear();
            impactPS.SetActive(true);
            impact = true;
        }
	}

}
