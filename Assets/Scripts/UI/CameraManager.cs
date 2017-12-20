using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Player;



public class CameraManager : MonoBehaviour {
	GameObject cameraPositionsObject;
	PlayerController player;
	Animator anim;
	Camera cam;
	Camera aimCam;
	Camera fpsactioncam;
	CamMode camMode;
	WeaponManager weaponManager;
	Transform[] camPositions;
	Transform[] fpsPositions;
	Transform[] tpsPositions;
	Inventory inventory;

	GameObject settings;
	//used for playing animations in fp
	Vector3 inHeadPos;


	Vector3 endPos;


	Vector3 fpsPerspective;
	float defaultFov;
	float targetFov;
	float animSpeed;
	public bool inFP;
	bool isAiming;
	bool camMoving;

	PlayerStance playerStance;
	PlayerStance lastStance;

	GameObject helmetSlot;

	// Use this for initialization


//IDEA FOR FP AIMING. attach camera to gameobject near weapon sights, on toggle, detach back to normal.


	void Start () {


		helmetSlot = GameObject.FindGameObjectWithTag ("HelmetSlot");
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController>();
		player.transform.GetComponent<Animator> ().enabled = true;
		player.enabled = true;
		inventory = player.gameObject.GetComponent<Inventory> ();
		weaponManager = player.GetComponent<WeaponManager> ();
		weaponManager.SniperScope.SetActive (false);
		weaponManager.Crosshair.SetActive (true);
		anim = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().anim;
		cam = Camera.main;
		settings = GameObject.Find ("Settings");
		if(settings){
			AudioListener.volume = settings.GetComponent<Settings> ().masterVol;
		}
			
		aimCam = GameObject.Find ("AimingCam").GetComponent<Camera>();
		aimCam.enabled = false;
		aimCam.gameObject.GetComponent<AudioListener> ().enabled = false;
		fpsactioncam = GameObject.Find ("fpsactioncam").GetComponent<Camera> ();
		fpsactioncam.enabled = false;
		fpsactioncam.gameObject.GetComponent<AudioListener> ().enabled = false;

		inFP = true;
		fpsPositions = new Transform[6];
		tpsPositions = new Transform[6];
		SetFPSCamPos ();
		SetTPSCamPos ();

		animSpeed = 10.0f;
		camMoving = false;
		playerStance = PlayerStance.Standing;
		lastStance = PlayerStance.Standing;
		camMode = CamMode.mainCam;
	}

	void SetFPSCamPos(){
		for (int i = 0; i < 3; ++i) {
			fpsPositions [i] = transform.parent.parent.GetChild (i);
		}
		//fpsPositions [3] = transform.parent.parent.GetChild (6).GetChild (0);
	}

	void SetTPSCamPos(){
		for (int i = 0; i < 3; ++i) {
			tpsPositions [i] = transform.parent.parent.GetChild (i+3);
		}
		//tpsPositions [3] = transform.parent.parent.GetChild (6).GetChild (1);
	}

	
	// Update is called once per frame
	void Update () {

		if (!player.gameObject.GetComponent<Status> ().isDead) {
			if (Input.GetKeyDown (KeyCode.V)) {
				ToggleCam ();
			}

			if (Input.GetMouseButtonDown (1)) {
				ToggleAim ();
			}

			UpdateCamPos ();
			SmoothCamTransition ();
			CheckCamMovement ();
		}
	}

	void UpdateCamPos(){
		
		if (!isAiming && cam.enabled) {
			if (inFP) {
				camPositions = fpsPositions;
			} else {
				camPositions = tpsPositions;
			}
	

			if (player.getProne ()) {
				transform.SetParent (camPositions [2]);
				playerStance = PlayerStance.Prone;
			} else if (player.getCrouch ()) {
				transform.SetParent (camPositions [1]);
				playerStance = PlayerStance.Crouching;
			} else {
				transform.SetParent (camPositions [0]);
				playerStance = PlayerStance.Standing;
			}

			//check for change in player stance, if so move cam
			if (playerStance != lastStance) {
				camMoving = true;
				lastStance = playerStance;
			}

		}

	}


	void ToggleCam(){
		if (isAiming) {
            
			ToggleAim ();
		} 
		if (!inFP) {
			cam.transform.localPosition = Vector3.zero;
		}
			
		cam.transform.localRotation = Quaternion.identity;
		inFP = !inFP;
		CullHelm ();
		camMoving = true;

	}

	void SmoothCamTransition(){
		if (camMoving) {
			switch (camMode) {
			case CamMode.mainCam:
				cam.transform.localPosition = Vector3.Slerp (cam.transform.localPosition, Vector3.zero, Time.deltaTime * animSpeed);
				break;
			case CamMode.aimCam:
				aimCam.transform.localPosition = Vector3.Slerp (aimCam.transform.localPosition, Vector3.zero, Time.deltaTime * animSpeed);
				break;
			case CamMode.fpsActionCam:
				fpsactioncam.transform.localPosition = Vector3.Slerp (fpsactioncam.transform.localPosition, Vector3.zero, Time.deltaTime * animSpeed);
				break;
			default:
				break;
			}
		}

	}

	void CheckCamMovement(){
		float buffer = 0.08f;
		Vector3 currentCamPos;
		switch (camMode) {
		case CamMode.mainCam:
			currentCamPos = cam.transform.localPosition;
			break;
		case CamMode.aimCam:
			currentCamPos = aimCam.transform.localPosition;
			break;
		case CamMode.fpsActionCam:
			currentCamPos = fpsactioncam.transform.localPosition;
			break;
		default:
			currentCamPos = cam.transform.localPosition;
			break;
		}

		if ((currentCamPos.x + buffer >= 0f && currentCamPos.x - buffer <= 0f) &&
		   	(currentCamPos.y + buffer >= 0f && currentCamPos.y - buffer <= 0f) &&
			(currentCamPos.z + buffer >= 0f && currentCamPos.z - buffer <= 0f)) {
			camMoving = false;
			switch (camMode) {
			case CamMode.mainCam:
				if (inFP) {
					cam.transform.localPosition = Vector3.zero;
				}
				break;
			case CamMode.aimCam:
				aimCam.transform.localPosition = Vector3.zero;
				break;
			case CamMode.fpsActionCam:
				fpsactioncam.transform.localPosition = Vector3.zero;
				break;
			default:
				break;
			}
		}

	}
	void ToggleAim(){
		GameObject weapon = weaponManager.GetEquippedWeapon ();
		if (weapon) {
            if (!weaponManager.SniperScope.activeSelf && weaponManager.GetEquippedWeapon().name.Contains("L96"))
            {
                weaponManager.Crosshair.SetActive(false);
                weaponManager.SniperScope.SetActive(true);
            }
            else
            {
                weaponManager.Crosshair.SetActive(true);
                weaponManager.SniperScope.SetActive(false);
            }
            Transform aimPos;
			if (inFP) {
				//aimPos = fpsPositions [3];
				aimPos = weapon.transform.GetChild(5);
			} else {
				//aimPos = tpsPositions [3];
				aimPos = weapon.transform.GetChild(6);
			}

			if (isAiming) {
				anim.SetBool ("ADS", false);
			} else {
				anim.SetBool("ADS", true);
				aimCam.fieldOfView = weapon.GetComponent<Gun> ().ironSightZoom;
			}

			aimCam.transform.SetParent (aimPos, false);
			aimCam.transform.localRotation = Quaternion.identity;
			isAiming = !isAiming;

			SwapCams ();
			//camMoving = true;
		}
	}

	void SwapCams(){
		if (!isAiming) {
			aimCam.enabled = false;
			cam.transform.localPosition = cam.transform.InverseTransformPoint(aimCam.transform.position);
			cam.transform.localRotation = Quaternion.identity;
			cam.enabled = true;
			camMode = CamMode.mainCam;
			camMoving = true;
		} else if (isAiming) {
			cam.enabled = false;
			aimCam.transform.localPosition = aimCam.transform.InverseTransformPoint(cam.transform.position);
			aimCam.enabled = true;
			camMode = CamMode.aimCam;
			camMoving = true;
		}

	}

	public bool GetAiming(){
		return isAiming;
	}
		
	public void ChangeWeapon(){
		if (!cam.enabled) {
			SwapCams ();
		}
	}

	public void CullHelm(){
		GameObject helmet = null;
		for (int i = 0; i < helmetSlot.transform.childCount; ++i) {
			if (helmetSlot.transform.GetChild(i).name.Equals("Helmet Slot") && helmetSlot.transform.GetChild(i).childCount > 0) {
				helmet = helmetSlot.transform.GetChild (i).GetChild(0).gameObject;
			}
		}

		if (inFP) {
			//cull helmet

			if (helmet) {
				helmet.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				Component[] meshRs = helmet.GetComponentsInChildren <MeshRenderer>();
				foreach (MeshRenderer meshRend in meshRs) {
					meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
				}
			}
		} else {
			//uncull helmet
			if (helmet) {
				helmet.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
				Component[] meshRs = helmet.GetComponentsInChildren <MeshRenderer>();
				foreach (MeshRenderer meshRend in meshRs) {
					meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
				}
			}
		}
	}

	public void Pickup(){
		if (inFP) {
			StartCoroutine (PickupRoutine ());
		}
	}

	IEnumerator PickupRoutine(){
		if (isAiming) {
			ToggleAim ();
		}

		Quaternion lastRot = cam.transform.localRotation;
		camMode = CamMode.fpsActionCam;

		cam.enabled = false;
		fpsactioncam.transform.localPosition = fpsactioncam.transform.InverseTransformPoint(cam.transform.position);
		fpsactioncam.transform.localRotation = cam.transform.localRotation*Quaternion.Euler(new Vector3(-14f, 0f, 0f));
		fpsactioncam.enabled = true;
		camMoving = true;

		yield return new WaitForSeconds(1.0f);

		camMode = CamMode.mainCam;

		fpsactioncam.enabled = false;
		cam.transform.localPosition = cam.transform.InverseTransformPoint (fpsactioncam.transform.position);
		cam.transform.localRotation = lastRot;
		cam.enabled = true;
		camMoving = true;
	}

	public void Death(Transform pos){

		StartCoroutine (DeathCam (pos));
	}

	IEnumerator DeathCam(Transform pos){
		if (isAiming) {
			SwapCams ();
		}
		float timer = 0f;
		float time = 1f;

		if (inFP) {
			cam.transform.SetParent (GameObject.FindGameObjectWithTag ("RagdollHead").transform);
			cam.transform.localPosition = new Vector3 (0f, 0f, 0.5f);
			cam.transform.localRotation = Quaternion.identity;
		} else {
			cam.transform.SetParent (null);
			Vector3 tpsstartpos = cam.transform.position;
			while (timer < time) {
				cam.transform.position = Vector3.Lerp (tpsstartpos, new Vector3 (cam.transform.position.x, cam.transform.position.y, cam.transform.position.z-1f), timer/time);
				cam.transform.LookAt (pos);
				timer += Time.deltaTime;
				yield return new WaitForEndOfFrame ();
			}
		}
		weaponManager.Crosshair.SetActive(false);
		weaponManager.SniperScope.SetActive(false);

	
		yield return new WaitForSeconds (2f);
		time = 5f;
		timer = 0f;
		cam.transform.SetParent (null);
		Vector3 startpos = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z-5f);
		while (timer < time) {
			cam.transform.position = Vector3.Lerp (startpos, new Vector3 (pos.position.x, pos.position.y + 5, pos.position.z-5f), timer/time);
			//cam.transform.position = new Vector3 (pos.x, pos.y + 10, pos.z);
			cam.transform.LookAt(pos);
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}
	}


	public void Win(Transform pos){

		StartCoroutine (WinCam (pos));
	}

	IEnumerator WinCam(Transform pos){
		
		if (isAiming) {
			SwapCams ();
		}
		weaponManager.Crosshair.SetActive(false);
		weaponManager.SniperScope.SetActive(false);
		player.transform.GetComponent<Animator> ().enabled = false;
		player.enabled = false;


		yield return new WaitForSeconds (0.5f);
		float time = 5f;
		float timer = 0f;
		cam.transform.SetParent (null);
		Vector3 startpos = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z-5f);
		while (timer < time) {
			cam.transform.position = Vector3.Lerp (startpos, new Vector3 (pos.position.x, pos.position.y + 5, pos.position.z-5f), timer/time);
			//cam.transform.position = new Vector3 (pos.x, pos.y + 10, pos.z);
			cam.transform.LookAt(pos);
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}

	}

	public void Damage(){
		if (inFP) {
			StartCoroutine (DamageShake ());
		}
	}

	public void Shoot(){
		if (inFP) {
			StartCoroutine (ShootShake ());
		}
	}

	IEnumerator DamageShake(){
		Camera activeCam;
		float shakeDuration = 0.5f;
		float shakeAmount = 0.1f;
		float decreaseFactor = 1.0f;

		if (camMode == CamMode.aimCam) {
			activeCam = aimCam;
		} else {
			activeCam = cam;
		}

		Vector3 originalPos = activeCam.transform.localPosition;


		while (shakeDuration > 0) {
			activeCam.transform.localPosition = Random.insideUnitSphere * shakeAmount;
			shakeDuration -= Time.deltaTime * decreaseFactor;
			yield return new WaitForEndOfFrame ();
		}

		activeCam.transform.localPosition = Vector3.zero;



	}

	IEnumerator ShootShake(){
		Camera activeCam;
		float shakeDuration = 0.5f;
		float shakeAmount = 0.01f;
		float decreaseFactor = 1.0f;
		float timer = shakeDuration;

		if (camMode == CamMode.aimCam) {
			activeCam = aimCam;
		} else {
			activeCam = cam;
		}
			
		Vector3 recoil = Random.insideUnitSphere * shakeAmount;
		Vector3 origPos = Vector3.zero;

		while (timer > 0) {
			activeCam.transform.localPosition = Vector3.Lerp (recoil, origPos, shakeDuration * Time.deltaTime);
			timer -= Time.deltaTime * decreaseFactor;
			yield return new WaitForEndOfFrame ();
		}

		activeCam.transform.localPosition = origPos;

	}
}



enum PlayerStance{
	Standing,
	Crouching,
	Prone
}

enum CamMode{
	fpsActionCam,
	mainCam,
	aimCam,
}
