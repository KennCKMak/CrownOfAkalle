using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float moveSpeed;	
	public float rotSpeed;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.W))
			this.transform.position += transform.forward * moveSpeed* Time.deltaTime;
		if (Input.GetKey (KeyCode.S))
			this.transform.position -= transform.forward * moveSpeed* Time.deltaTime;
		if (Input.GetKey (KeyCode.A))
			this.transform.position -= transform.right * moveSpeed* Time.deltaTime;
		if (Input.GetKey (KeyCode.D))
			this.transform.position += transform.right * moveSpeed * Time.deltaTime;
			
		if (Input.GetKey (KeyCode.Q))
			this.transform.Rotate (Vector3.down * rotSpeed * Time.deltaTime);
		if (Input.GetKey (KeyCode.E))
			this.transform.Rotate (Vector3.up * rotSpeed * Time.deltaTime);
		
	}
}
