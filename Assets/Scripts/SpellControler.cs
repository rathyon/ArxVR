using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellControler : MonoBehaviour {

    public Vector3 direction;
    public GameObject impactPS;
    //public GameObject reticleDirection;
    public float time = 0;
    public bool impact;
    public Vector3 velocity;

	// Use this for initialization
	void Start () {
        impact = false;
	}
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if (impact){
            impactPS.SetActive(true);

            Destroy(gameObject);
        }
        if(time >= 5.0){
            Destroy(gameObject);
        }
        transform.position += direction * 0.5f;
	}

	private void OnCollisionEnter(Collision collision)
	{
        impact = true;
	}

}
