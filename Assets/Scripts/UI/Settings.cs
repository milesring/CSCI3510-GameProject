using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
	public float masterVol;
	public float musicVol;
	public float sfxVol;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
	}

	public void ChangeMasterVolume(Slider slider){
		masterVol = slider.value;
		Debug.Log ("master: "+masterVol);
	}

	public void ChangeMusicVolume(Slider slider){
		musicVol = slider.value;
		Debug.Log ("music: "+musicVol);
	}

	public void ChangeSFXVolume(Slider slider){
		sfxVol = slider.value;
		Debug.Log ("sfx: "+sfxVol);

	}
		
}
