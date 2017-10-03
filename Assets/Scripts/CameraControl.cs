using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public float moveSpeed;	
	public float minMoveSpeed = 2;
	public float maxMoveSpeed = 10;
	public float rotSpeed = 90;
	float scrollSpeed = 4.0f;

	public float heightDest;
	public float heightMax = 10f;
	public float heightMid = 4.5f;
	public float heightMin = 1.7f;

	public float upperEulerAngle = 67.615f;
	public float lowerEulerAngle = 22.690f;

	public float targetEulerAngle;
	float percentage;

	void Start(){
		heightDest = transform.position.y;


	}

	// Update is called once per frame
	void Update () {
		heightDest -= Input.GetAxis ("Mouse ScrollWheel") * scrollSpeed/2;
		heightDest = Mathf.Clamp (heightDest, heightMin, heightMax);
		Vector3 newPos =  new Vector3(transform.position.x, heightDest, transform.position.z);
		transform.position = Vector3.Lerp(transform.position, newPos, scrollSpeed*Time.deltaTime);

		if (transform.position.y <= heightMid) {
			percentage = (transform.position.y - heightMin) / (heightMid - heightMin);
			targetEulerAngle = percentage * (upperEulerAngle - lowerEulerAngle) + lowerEulerAngle;

			//transform.GetChild (0).transform.Rotate (Vector3.right * targetEulerAngle * Time.deltaTime);
			transform.GetChild (0).transform.localRotation = Quaternion.Euler(targetEulerAngle, 0.0f, 0.0f);
		} else {
			percentage = 1.00f;
			//transform.GetChild(0).transform.rotation = Quaternion.AngleAxis (upperEulerAngle, Vector3.right);
			transform.GetChild (0).transform.localRotation = Quaternion.Euler(upperEulerAngle, 0.0f, 0.0f);
		}

		moveSpeed = Mathf.Clamp ((maxMoveSpeed - minMoveSpeed) * percentage + minMoveSpeed, minMoveSpeed, maxMoveSpeed);


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
