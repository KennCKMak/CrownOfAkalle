using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	protected float moveSpeed = 8;
	protected float minMoveSpeed = 2;
	protected float maxMoveSpeed = 10;
	protected float rotSpeed = 90;
	float scrollSpeed = 5.0f;

	float percentage;
	protected float heightDest;
	protected float heightMax = 12f;
	protected float heightMid = 6.5f;
	protected float heightMin = 1.7f;
	protected float upperEulerAngle = 56.0f;
	protected float lowerEulerAngle = 22.690f;
	protected float targetEulerAngle;

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
