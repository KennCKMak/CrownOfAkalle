using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonCommands : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SwitchLevel(string s)
    {
        try
        {
            SceneManager.LoadScene(s, LoadSceneMode.Single);
        }
        catch
        {
            Debug.LogWarning("Failed to load scene " + s);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
