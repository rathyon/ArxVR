﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellControler : MonoBehaviour {

    public Vector3 direction;
    public GameObject impactPS;
    //public GameObject reticleDirection;
    public float time = 0;
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
        transform.position += direction * 0.1f;
	}

	private void OnCollisionEnter(Collision collision)
	{
        if(!collision.gameObject.CompareTag("Player")||!collision.gameObject.CompareTag("MainCamera")){
            impactPS.SetActive(true);
            Destroy(gameObject);
        }
	}

}
