using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	public CameraState state;
	public enum CameraState { Strategy, Simulation }

	public GameObject CameraStrategy;
	public GameObject CameraSimulation;


	void Start(){
		CameraStrategy = transform.FindChild ("CameraStrategy").gameObject;
		CameraSimulation = transform.FindChild ("CameraSimulation").gameObject;


		CameraStrategy.transform.parent = null;
		CameraSimulation.transform.parent = null;

		setCameraState (CameraState.Strategy);
	}

	void Update(){

	}

	public CameraState getCameraState(){
		return state;
	}

	public void setCameraState(CameraState newState){
		if (state != newState) {
			state = newState;
			UpdateState ();
		}
	}

	public void UpdateState(){
		switch (state) {
		case CameraState.Strategy:
			CameraStrategy.SetActive (true);
			CameraSimulation.SetActive (false);
			break;
		case CameraState.Simulation:
			CameraStrategy.SetActive (false);
			CameraSimulation.SetActive (true);
			break;
		default:
			break;
		}
	}

}
