using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour {
    public Animator anim;

	public float damage = 10;
	public float range = 100f;
	public float fireRate = 15f;
	public float impactForce = 30f;
	public float ironSightZoom = 60f;
	private int bulletType;

	public int maxAmmo = 10;
	private int currentAmmo;
	public float reloadTime = 1;
	public bool isReloading;

	public Camera fpsCam;
	public GameObject muzzleFlash;
	public GameObject impactEffect;
    
	//public Animator animator;

	public AudioClip shotSound;
	public AudioClip reloadSound;
	private AudioSource audioSource;

	private float distanceToBarrel;

	// Use this for initialization
	void Start () {
        currentAmmo = 0;
		isReloading = false;
		anim = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
		audioSource = GetComponent<AudioSource> ();
        //Assign values based on gun model
        switch (name)
        {
		case "M4_Carbine":
		case "M4_Carbine(Clone)":
			bulletType = 0;
                //rightHandTrigger.transform.localPosition = new Vector3(0.009f, -0.157f, 0.146f);
                //leftHandBarrel.transform.localPosition = new Vector3(-0.029f, -0.1442f, 0.4627f);
				//rightHandTrigger.transform.localPosition = transform.GetChild (1).localPosition;
				//leftHandBarrel.transform.localPosition = transform.GetChild (2).localPosition;
                break;
            case "AK-47":
            case "AK-47(Clone)":
                bulletType = 1;
                //change these
                //rightHandTrigger.transform.localPosition = new Vector3(0.009f, -0.157f, 0.146f);
                //leftHandBarrel.transform.localPosition = new Vector3(-0.029f, -0.1442f, 0.4627f);
                break;
            case "L96_Sniper_Rifle":
            case "L96_Sniper_Rifle(Clone)":
                bulletType = 2;
                //change these
				//rightHandTrigger.transform.localPosition = new Vector3(0.009f, -0.157f, 0.146f);
				//leftHandBarrel.transform.localPosition = new Vector3(-0.029f, -0.1442f, 0.4627f);
                break;
            default:
                bulletType = 3;
                break;
        }
	}

	void OnEnable(){
		isReloading = false;
	}

	public IEnumerator Reload(int[] bullets, Inventory inv, bool sound){
		isReloading = true;
		if (sound) {
			PlayReloadSound ();
		}
        //0.25f for animation transitions
        yield return new WaitForSeconds(reloadTime - 0.25f);
        //gun not empty
        if (currentAmmo > 0) {
			int amountToReload = maxAmmo - currentAmmo;

			//if the player doesn't have enough ammo to fill the gun
			if (bullets [bulletType] < amountToReload) {
				currentAmmo += bullets [bulletType];
				bullets [bulletType] = 0;
			} 
			//player has enough ammo
			else {
				currentAmmo += amountToReload;
				bullets [bulletType] -= amountToReload;
			}
		} 
		//gun empty
		else {
			if (bullets [bulletType] < maxAmmo) {
				currentAmmo = bullets [bulletType];
				bullets [bulletType] = 0;
			} else {
				currentAmmo = maxAmmo;
				bullets [bulletType] -= maxAmmo;
			}
		}
		inv.UpdateAmmoInInventory ();
        yield return new WaitForSeconds(1f);
        isReloading = false;
    }

	public void PlayShotSound(){
		audioSource.clip = shotSound;
		audioSource.Play ();
	}

	public void PlayReloadSound(){
		audioSource.clip = reloadSound;
		audioSource.PlayDelayed (1.5f);
	}

	public void StopSound(){
		audioSource.clip = null;
	}
	public float GetRange(){
		return range;
	}

	public float GetDamage(){
		return damage;
	}

	public float GetImpactForce(){
		return impactForce;
	}

	public int GetCurrentAmmo(){
		return currentAmmo;
	}

	public int GetMaxAmmo(){
		return maxAmmo;
	}

	public void DecreaseAmmo(){
		currentAmmo--;
	}

	public bool GetReloading(){
		return isReloading;
	}

	public float GetFireRate(){
		return fireRate;
	}

	public AudioClip GetShotSound(){
		return shotSound;
	}

	public AudioClip GetReloadSound(){
		return reloadSound;
	}

	public int GetAmmoType(){
		return bulletType;
	}
}
