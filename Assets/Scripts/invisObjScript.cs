using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class invisObjScript : MonoBehaviour {

	protected GameObject followTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (followTarget) {
			transform.position = Vector3.Lerp (transform.position, followTarget.transform.position, 5.0f * Time.deltaTime);
		}
	}

	public void setFollowTarget(GameObject target){
		transform.position = target.transform.position;
		followTarget = target;
	}

	public void stopFollow(){
		followTarget = null;
	}
}
