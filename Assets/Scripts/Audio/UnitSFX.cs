using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSFX : MonoBehaviour {

	// Use this for initialization
	[HideInInspector] public AudioManager audioManager;


	private AudioSource sourceWalk;
	private AudioSource sourceAttack;
	private AudioSource sourceDamaged;

	public int totalAudioSources = 1;

	[HideInInspector] public bool sfxIsInitialized = false;

	void Awake () {
		sourceWalk = gameObject.AddComponent<AudioSource> ();
		sourceAttack = gameObject.AddComponent<AudioSource> ();
		sourceDamaged = gameObject.AddComponent<AudioSource> ();
	}

	public void SetUpClip(string walkSoundName, string attackSoundName, string damagedSoundName){
		SetUpWalkClip (walkSoundName);
		SetUpAttackClip (attackSoundName);
		SetUpDamagedSound (damagedSoundName);
	}

	public void SetUpWalkClip(string walkSoundName){
		Sound walkSound = audioManager.RequestSFX (walkSoundName);
		if (walkSound != null) {
			sourceWalk.clip = walkSound.clip;
			sourceWalk.volume = walkSound.volume / totalAudioSources;
			sourceWalk.pitch = walkSound.pitch;
			sourceWalk.spatialBlend = 1.0f;
		}

	}

	public void SetUpAttackClip(string attackSoundName){
		Sound attackSound = audioManager.RequestSFX (attackSoundName);

		if (attackSound != null) {
			sourceAttack.clip = attackSound.clip;
			sourceAttack.volume = attackSound.volume/totalAudioSources;
			sourceAttack.pitch = attackSound.pitch;
			sourceAttack.spatialBlend = 1.0f;
		}

	}

	public void SetUpDamagedSound(string damagedSoundName){
		Sound damagedSound = audioManager.RequestSFX (damagedSoundName);
		if (damagedSound != null) {
			sourceDamaged.clip = damagedSound.clip;
			sourceDamaged.volume = damagedSound.volume/totalAudioSources;
			sourceDamaged.pitch = damagedSound.pitch;
			sourceDamaged.spatialBlend = 1.0f;
		}
	}

	public void PlayWalkSFX(){
		sourceWalk.Play ();
	}

	public void PlayAttackSFX(){
		sourceAttack.Play ();
	}

	public void PlayDamagedSFX(){
		sourceDamaged.Play ();
	}

	public void GetUnitInformation(Unit unit){
		if (unit.isMounted())
			SetUpWalkClip ("Horsestep");
		else
			SetUpWalkClip ("Footstep");

		if (!unit.isRanged())
			SetUpAttackClip ("SwordAttack1");
		else
			SetUpAttackClip ("ArcherAttack1");

		SetUpDamagedSound ("Damaged");

		

		sfxIsInitialized = true;
	}

	public void GetUnitSimInformation(UnitSim unit, int otherAllies){
		totalAudioSources = otherAllies*2;
		if (unit.isMounted())
			SetUpWalkClip ("Horsestep");
		else
			SetUpWalkClip ("Footstep");

		if (unit.getMeleeWeaponType() == Unit.MeleeWeaponType.Sword)
			SetUpAttackClip ("SwordAttack1");
		else
			SetUpAttackClip ("ArcherAttack1"); //debugging purposes only

		SetUpDamagedSound ("Damaged");

		sfxIsInitialized = true;
	}

}
