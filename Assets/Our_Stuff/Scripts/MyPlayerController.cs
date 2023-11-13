using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : MonoBehaviour {

    public float speed;

    private Rigidbody rb; 

	// Use this for initialization
	void Start () { 
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(moveHorizontal*Time.deltaTime*speed, 0f, moveVertical*Time.deltaTime*speed);

        transform.Translate(movement); //Translate ou rb.addForce?
    }

}
