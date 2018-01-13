using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	protected bool inputEnabled;
	protected bool focusing;
	protected GameObject focusTarget;

	protected bool following;
	protected GameObject followTarget;
	public GameObject invisObjPrefab;


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


	private Vector3 startPosition;
	private Quaternion startRotation;

	void Start(){
		startPosition = transform.position;
		startRotation = transform.rotation;

		heightDest = transform.position.y;
		inputEnabled = true;
		focusing = false;
		following = false;
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

		if (Input.GetKey (KeyCode.W)) 
			this.transform.position += transform.forward * moveSpeed * Time.deltaTime;
		if (Input.GetKey (KeyCode.S)) 
			this.transform.position -= transform.forward * moveSpeed * Time.deltaTime;
		if (Input.GetKey (KeyCode.A)) 
			this.transform.position -= transform.right * moveSpeed * Time.deltaTime;
		if (Input.GetKey (KeyCode.D)) 
			this.transform.position += transform.right * moveSpeed * Time.deltaTime;

		if (focusing) {
			Focus ();
			return;
		}

		if (following) {
			Follow ();
		}

		if (Input.GetKey (KeyCode.Q))
			this.transform.Rotate (Vector3.down * rotSpeed * Time.deltaTime);
		if (Input.GetKey (KeyCode.E))
			this.transform.Rotate (Vector3.up * rotSpeed * Time.deltaTime);

	}

	public void ResetHeight(){
		transform.position = new Vector3 (transform.position.x, startPosition.y, transform.position.z);
	}

	public void ResetPosition(){
		transform.position = startPosition;
	}
	public void ResetRotation(){
		transform.rotation = startRotation;
	}

	public void SetPosition(Vector3 newPos){
		transform.position = newPos;
	}

	public void FocusAt(GameObject target){
		this.focusTarget = target;
		inputEnabled = false;
		focusing = true;
		DelayStopFocus (10.0f);
	}

	public void Focus(){
		if (!focusTarget)
			StopFocus ();

		Vector3 targetPosition = focusTarget.transform.position - transform.position;
		targetPosition.y = 0;
		Quaternion lookRot = Quaternion.LookRotation (targetPosition);
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 2.5f * Time.deltaTime);
		//if((target.transform.position - transform.position).magnitude > 5.0f)
			//transform.position = Vector3.Lerp (transform.position, target.transform.position, 5.0f*Time.deltaTime);

	}

	public void DelayStopFocus(float time){
		Invoke ("StopFocus", time);
	}

	public void StopFocus(){
		inputEnabled = true;
		focusing = false;

	}

	public void FollowTarget(GameObject target){
		following = true;

		GameObject invisObj = Instantiate (invisObjPrefab, target.transform.position, Quaternion.identity) as GameObject;
		invisObj.GetComponent<invisObjScript> ().setFollowTarget (target);

		transform.parent = invisObj.transform;
		followTarget = invisObj; // a way to destroy the obj later...
		transform.localPosition = new Vector3(-0.7f, 5.43f, -3.6f);
	}

	public void Follow(){
		
	}

	public void StopFollow(){
		following = false;

		transform.parent = null;
		Destroy (followTarget);

		followTarget = null;
	}

	public bool isInputEnabled(){return inputEnabled;}




}
