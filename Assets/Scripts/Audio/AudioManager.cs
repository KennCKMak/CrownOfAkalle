﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

//AudioManager - Screen, UI, BGM stuff
public class AudioManager : MonoBehaviour {

	string currentSceneName;
	public bool muteBGM;
	public bool muteSFX;
	public BGM[] bgm;
	public Sound[] sfx;
	public static AudioManager instance;


	public enum bgmSongVersion
	{
		Stream,
		Rage

	};

	public AudioSource bgmSource = null;
	public bgmSongVersion bgmCurrentVersion = bgmSongVersion.Stream; //stream, rage
	public BGM currentBGM = null;

	void Awake () {
		if (instance == null){
			instance = this;
		} else {
			Destroy (gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);

		bgmSource = transform.GetChild (0).gameObject.AddComponent<AudioSource> ();

		foreach (Sound s in sfx) {
			s.source = transform.GetChild(1).gameObject.AddComponent<AudioSource> ();
			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
		}
		currentSceneName = "";
	}

    private void Start()
    {
        CheckScene(SceneManager.GetActiveScene());
    }

    

	void Update() {
        SceneManager.sceneLoaded += OnSceneLoaded;
	}
    
    public void CheckScene(Scene scene)
    {
		if (currentSceneName != scene.name) {
			StopBGM ();
		} else
			return;

		currentSceneName = SceneManager.GetActiveScene ().name;
        switch (scene.name)
        {
            case "MainGame"://test
                PlayBGM("E Pluribus Unum", bgmSongVersion.Stream);
                return;
			case "MainMenu"://main menu
                PlayBGM("Camelot", bgmSongVersion.Stream);
                return;
            default:
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene(scene);
    }

    public void PlaySFX(string soundName){
        if (muteSFX)
            return;
		Sound s = Array.Find (sfx, sound => sound.name == soundName);
		if (s == null)
			Debug.LogWarning ("Sound: " + soundName + " was not found");
		else {
			s.source.Play ();
			//Debug.Log ("Playing " + soundName);
		}
	}


	public void PlayBGM(string bgmName, bgmSongVersion songType)
	{
		bgmSource.time = 0.0f;
        BGM music = Array.Find (bgm, bgm => bgm.name == bgmName);
		if (music == null)
			Debug.LogWarning ("BGM: " + bgmName + " was not found");
		else {
			currentBGM = music;
			if (songType == bgmSongVersion.Stream) {
				bgmSource.clip = music.clip1;
				bgmSource.volume = music.volume1;
				bgmSource.pitch = music.pitch1;
			} else {
				bgmSource.clip = music.clip2;
				bgmSource.volume = music.volume2;
				bgmSource.pitch = music.pitch2;
			}
			bgmSource.loop = true;
				bgmSource.Play ();
		}

		if (muteBGM)
			bgmSource.Stop ();
	}

	public void PlayBGM(string bgmName)
	{
		bgmSource.time = 0.0f;
		BGM music = Array.Find (bgm, bgm => bgm.name == bgmName);
		if (music == null)
			Debug.LogWarning ("BGM: " + bgmName + " was not found");
		else {
			currentBGM = music;
			bgmSource.clip = music.clip1;
			bgmSource.volume = music.volume1;
			bgmSource.pitch = music.pitch1;

			bgmSource.loop = true;
			bgmSource.Play ();
		}

		if (muteBGM)
			bgmSource.Stop ();
	}

	public void PlayBGM(){

		bgmSource.time = 0.0f;
		if (muteBGM) {
			bgmSource.Stop ();
			return;
		}
		bgmSource.Play();
	}

	public void SwitchBGM()
    {
        float currentTime = bgmSource.time;
		float currentLength = bgmSource.clip.length;

		if (bgmCurrentVersion == bgmSongVersion.Stream) {
			//Switch to Rage
			bgmCurrentVersion = bgmSongVersion.Rage;

			bgmSource.clip = currentBGM.clip2;
			bgmSource.volume = currentBGM.volume2;
			bgmSource.pitch = currentBGM.pitch2;

			bgmSource.time = (currentTime / currentLength) * currentBGM.clip2.length;

		} else {
			//Switch to Stream
			bgmCurrentVersion = bgmSongVersion.Stream;

			bgmSource.clip = currentBGM.clip1;
			bgmSource.volume = currentBGM.volume1;
			bgmSource.pitch = currentBGM.pitch1;

			bgmSource.time = (currentTime / currentLength) * currentBGM.clip1.length;

		}
		if (muteBGM) {
			bgmSource.Stop ();
			return;
		}

		bgmSource.Stop ();
		bgmSource.Play ();
	}

	public void SwitchBGMTo(bgmSongVersion newSongVersion)
    {
        if (bgmCurrentVersion == newSongVersion)
			return;
        float currentTime = 0;
        if (bgmSource == null)
        {
        }


        currentTime = bgmSource.time;
		float currentLength = bgmSource.clip.length;

		if (newSongVersion == bgmSongVersion.Rage) {
			//Switch to Rage
			bgmCurrentVersion = bgmSongVersion.Rage;

			bgmSource.clip = currentBGM.clip2;
			bgmSource.volume = currentBGM.volume2;
			bgmSource.pitch = currentBGM.pitch2;

			bgmSource.time = (currentTime / currentLength) * currentBGM.clip2.length;

		} else {
			//Switch to Stream
			bgmCurrentVersion = bgmSongVersion.Stream;

			bgmSource.clip = currentBGM.clip1;
			bgmSource.volume = currentBGM.volume1;
			bgmSource.pitch = currentBGM.pitch1;

			bgmSource.time = (currentTime / currentLength) * currentBGM.clip1.length;

		}
		if (muteBGM) {
			bgmSource.Stop ();
			return;
		}
		bgmSource.Stop ();
		bgmSource.Play ();
	}

	public void StopBGM(){
		bgmSource.Stop ();
	}

	public Sound RequestSFX(string soundClip) {
		Sound s = Array.Find (sfx, sound => sound.name == soundClip);
		if (s == null) {
			Debug.LogWarning ("Sound: " + soundClip + " was not found");
		} else
			return s;
		return null;
	}

	public void ToggleMuteBGM(){
		if (!muteBGM) {
			muteBGM = true;
			StopBGM ();
		} else {
			muteBGM = false;
			PlayBGM ();
		}
	}

	public void ToggleMuteSFX(){
		muteSFX = !muteSFX;
	}
}
