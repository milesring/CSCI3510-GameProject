using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Player;

namespace Assets.Scripts.Player
{
    public class WeaponManager : MonoBehaviour
    {
        public GameObject SniperScope;
        public GameObject Crosshair;
        public GameObject grenadePrefab;
		public AudioClip punchClip;
		CameraManager camManager;
        GameObject lookDir;
        public int selectedWeapon = 0;
        protected Inventory inventory;
        protected GameObject equippedWeapon;
        public GameObject fist;
		public GameObject playerHead;
        Camera cam;
        protected GameObject aim;
        IKControl ik;
        protected Animator anim;
        AudioSource audioSource;
        int mask;
        float nextTimeToFire = 0f;
		float nextTimeToThrow = 0f;
		float grenadeThrowRate = 1f;
		float throwForce = 600f;
		bool punchSoundPlayed;

		Quaternion offsetYAngle;
		public float yOffsetAmount;
		Quaternion offsetXAngle;
		public float xOffsetAmount;

		GameStatus gameStatus;

        // Use this for initialization
        void Start()
        {
			//playerHead = GameObject.FindGameObjectWithTag ("HelmetSlot");
			audioSource = gameObject.GetComponent<AudioSource> ();
            ik = GetComponent<IKControl>();
            anim = GetComponent<Animator>();
            SetEquippedWeapon(null);
            aim = new GameObject();

            Initilize();

            equippedWeapon = null;
			punchSoundPlayed = false;
        }

        protected virtual void Initilize() {
            lookDir = new GameObject();
            lookDir.name = "LookDirectionObject";
            lookDir.transform.localPosition = playerHead.transform.position;
            lookDir.transform.localRotation = aim.transform.rotation;
            //lookDir.transform.localRotation = Quaternion.Euler(0f,0f,0f);
            lookDir.transform.Translate(0, 0, 5f);

            cam = Camera.main;
            aim.transform.position = cam.transform.position;
            aim.transform.rotation = cam.transform.rotation;
            aim.transform.SetParent(cam.transform);

            mask = 1 << 8;
            mask = ~mask;

            camManager = cam.GetComponent<CameraManager>();
            inventory = GetComponent<PlayerInventory>();

			gameStatus = GameObject.Find ("GameStatus").GetComponent<GameStatus> ();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
			if (!gameStatus.GameOver) {
				CheckWeaponSwitch ();
				CheckFire ();   
				UpdateLookDirection ();
			}
        }

        protected virtual void FixedUpdate(){
			if (!gameStatus.GameOver) {
				UpdateIK ();
				if (!anim.GetCurrentAnimatorStateInfo (4).IsName ("Grenade"))
					CheckGrenade ();
				CheckReload ();
			}
        }
        private void UpdateLookDirection()
        {
//            lookDir.transform.localPosition = cam.transform.position;
//            lookDir.transform.localRotation = cam.transform.rotation;
			lookDir.transform.position = playerHead.transform.position;
			lookDir.transform.rotation = cam.transform.rotation;
            lookDir.transform.Translate(0, 0, 5f);
            if (equippedWeapon)
            {
                inventory.equippedWeaponSlot.transform.LookAt(lookDir.transform);
                //inventory.equippedWeaponSlot.transform.Rotate(90, 0, 0);
                equippedWeapon.transform.LookAt(lookDir.transform);
                equippedWeapon.transform.Rotate(0, 90, 0);
            }
        }

		protected void UpdateIK(){
			//Set ik goals to the weapon grip position and rotation
			if (equippedWeapon) {
				ik.rHandpos = equippedWeapon.transform.GetChild (1).position;
				ik.rHandrot = equippedWeapon.transform.GetChild (1).rotation;
				ik.lHandpos = equippedWeapon.transform.GetChild (2).position;
				ik.lHandrot = equippedWeapon.transform.GetChild (2).rotation;
                ik.rElbowpos = equippedWeapon.transform.GetChild(3).position;
                ik.lElbowpos = equippedWeapon.transform.GetChild(4).position;
			}
		}

        void CheckFire()
        {
			if (equippedWeapon != null) {
				if (Input.GetMouseButton (0)) {
					Shoot ();
				} else
					anim.SetBool ("Shooting", false);
			} else if (Input.GetMouseButtonDown (0)) {
				
				Punch ();
			}
        }

        void CheckGrenade()
        {
            //check if player has a grenade here later
            if (Input.GetKeyDown(KeyCode.G))
            {
                StartCoroutine(ThrowGrenade());
            }

        }

        void CheckReload()
        {
            if (Input.GetKeyDown(KeyCode.R) && equippedWeapon != null)
            {
                ReloadWeapon();
            }
        }

        public void ReloadWeapon() {
            Gun weapon = equippedWeapon.GetComponent<Gun>();
            if (!weapon.GetReloading()) {
                anim.SetBool("Interact", true);
                anim.SetTrigger("Reload");
				bool sound = false;
				if (CompareTag ("Player")) {
					sound = true;
				}
                StartCoroutine(weapon.Reload(inventory.GetAmmo(), inventory, sound));
            }
        }

        protected void Punch(){
			if (Time.time >= nextTimeToFire)
			{
				anim.SetTrigger ("Punch");
				nextTimeToFire = Time.time + 1;
				StartCoroutine (PunchCalculation ());
			}

		}

		IEnumerator PunchCalculation(){
			punchSoundPlayed = true;
			yield return new WaitForSeconds (0.5f);


            RaycastHit hit = new RaycastHit();
            RaycastHit[] hits = Physics.RaycastAll(fist.transform.position, aim.transform.forward, 2.5f);
            Array.Sort(hits, SortByDistance);
            for (int x = 0; x < hits.Length; x++)
                if (hits[x].collider.isTrigger == false && hits[x].collider.gameObject != this.gameObject) {
                    hit = hits[x];
                    break;
                }
            if (hit.collider != null) {
				audioSource.clip = punchClip;
				if (gameObject.CompareTag ("Player")) {
					audioSource.Play ();
				}
				//applies force to object away from player
				if (hit.rigidbody)
				{
					hit.rigidbody.AddForce(-hit.normal * 50f);
				}

				//checks if target is an enemy and applies damage
				Status targetStatus = hit.transform.GetComponent<Status> ();
				if (targetStatus) {
					if (targetStatus.DamageHealthAndArmor (10f)) {
						Status status = transform.GetComponent<Status> ();
						if (status != null) {
							status.IncreaseKills ();
						}
					}
				}
			}
			punchSoundPlayed = false;
		}

        protected void Shoot()
        {
            if (equippedWeapon.GetComponent<Gun>().GetCurrentAmmo() > 0 && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / equippedWeapon.GetComponent<Gun>().GetFireRate();
                anim.SetBool("Shooting", true);

                if (GetComponent<AIWeaponManager>() == null)
    				camManager.Shoot ();

                equippedWeapon.GetComponent<Gun>().PlayShotSound();
                GameObject muzzleFlash = Instantiate(equippedWeapon.GetComponent<Gun>().muzzleFlash, equippedWeapon.transform.GetChild(0).transform.position, equippedWeapon.transform.GetChild(0).transform.rotation);
                Destroy(muzzleFlash, 0.1f);

                Vector3 camBack = aim.transform.forward.normalized;
                camBack.Scale(new Vector3(-1f, -1f, -1f));

                equippedWeapon.GetComponent<Gun>().DecreaseAmmo();
					
//				if (equippedWeapon.name.Contains ("Sniper")) {
//					yOffsetAmount = -2.22f;
//					xOffsetAmount = -0.84f;
//				} else if (equippedWeapon.name.Contains ("AK-47")) {
//					yOffsetAmount = -1.49f;
//					xOffsetAmount = -1.54f;
//				} else {
//					yOffsetAmount = -1.49f;
//					xOffsetAmount = -1.38f;
//				}
					//offsetYAngle = Quaternion.AngleAxis (yOffsetAmount, new Vector3 (0, 1, 0));
					offsetYAngle = Quaternion.Euler (0f, yOffsetAmount, 0f);
					offsetXAngle = Quaternion.Euler (xOffsetAmount, 0f, 0f);
					//offsetXAngle = Quaternion.AngleAxis (xOffsetAmount, new Vector3 (1, 0, 0));
					//Vector3 shootVector = offsetXAngle * offsetYAngle * aim.transform.forward;
				Vector3 shootVector = offsetXAngle * offsetYAngle * equippedWeapon.transform.GetChild (0).transform.forward;

                //              if (Physics.Raycast(equippedWeapon.transform.GetChild(0).transform.position, shootVector,
                //			        out hit, equippedWeapon.GetComponent<Gun>().GetRange(), mask))
                //				{
                RaycastHit hit = new RaycastHit();
                RaycastHit[] hits = Physics.RaycastAll(equippedWeapon.transform.GetChild(0).transform.position, shootVector, equippedWeapon.GetComponent<Gun>().GetRange());
                Array.Sort(hits, SortByDistance);
                for (int x = 0; x < hits.Length; x++) {
                    Debug.Log(hits[x].collider.gameObject);
                    Debug.Log("trigger test: " + (hits[x].collider.isTrigger == false));
                    Debug.Log("this.gameobject: " + (hits[x].collider.gameObject != this.gameObject));

                    if (hits[x].collider.isTrigger == false && hits[x].collider.gameObject != this.gameObject && !hits[x].collider.gameObject.CompareTag("Gun")) {
                        Debug.Log("This one");
                        hit = hits[x];
                        break;
                    }
                }

                //Debug.Log(hit.collider);
                //Debug.Log(hit.collider.gameObject);

                if (hit.collider != null) {

                    //Debug.Log (hit.transform.name);


                    Target target = hit.transform.GetComponent<Target>();
                    if (target != null)
                    {
                        target.TakeDamage(equippedWeapon.GetComponent<Gun>().GetDamage());
                    }

                    //applies force to object away from player
                    if (hit.rigidbody != null)
                    {
                        hit.rigidbody.AddForce(-hit.normal * equippedWeapon.GetComponent<Gun>().GetImpactForce());
                    }

					//checks if target is an enemy and applies damage
					Status targetStatus = hit.transform.GetComponent<Status> ();
					if (targetStatus) {
						if (targetStatus.DamageHealthAndArmor (equippedWeapon.GetComponent<Gun> ().GetDamage ())) {
							Status status = transform.GetComponent<Status> ();
							if (status != null) {
								status.IncreaseKills ();
							}
						}
					}

                    GameObject impactGO = Instantiate(equippedWeapon.GetComponent<Gun>().impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 0.2f);
                }
            }

            if (equippedWeapon.GetComponent<Gun>().GetCurrentAmmo() < 1)
            {
                anim.SetBool("Shooting", false);
            }
        }

        int SortByDistance(RaycastHit r1, RaycastHit r2) { // radius biggest to small
            return r1.distance.CompareTo(r2.distance);
        }

        IEnumerator ThrowGrenade()
        {
			if (inventory.FindAndConsumeItem("Grenade") && Time.time >= nextTimeToThrow)
            {
                if (equippedWeapon)
                {
                    SelectWeapon();
                    yield return new WaitForSeconds(0.567f);
                }
                anim.SetTrigger("Grenade");
                yield return new WaitForSeconds(1.8f);
                nextTimeToThrow = Time.time + 1f / grenadeThrowRate;
				GameObject grenade = Instantiate (grenadePrefab, inventory.equippedWeaponSlot.transform.position + new Vector3(0,0.2f,0.11f), transform.rotation);
				Rigidbody rb = grenade.GetComponent<Rigidbody> ();
				rb.AddForce ((aim.transform.forward + new Vector3(0,0.5f,0)) * throwForce);
				StartCoroutine (grenade.GetComponent<Grenade> ().Detonate (gameObject));
                yield return new WaitForSeconds(1f);
                if (equippedWeapon==null && inventory.GetWeapons()[selectedWeapon] != null)
                    SelectWeapon();
			}

        }

        void CheckWeaponSwitch()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && inventory.GetWeapons()[0] != null)
            {
                //Debug.Log ("Selecting weapon slot 1");
                selectedWeapon = 0;
                SelectWeapon();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && inventory.GetWeapons()[1] != null)
            {
                //Debug.Log ("Selecting weapon slot 2");
                selectedWeapon = 1;
                SelectWeapon();
            }
        }

        void ScrollWeapons(float mouse)
        {
            if (mouse > 0f)
            {
                selectedWeapon++;
                if (selectedWeapon > 1)
                {
                    selectedWeapon = 0;
                }
            }

            if (mouse < 0f)
            {
                selectedWeapon--;
                if (selectedWeapon < 0)
                {
                    selectedWeapon = inventory.GetWeapons().Length - 1;
                }
            }
        }

        public void SelectWeapon()
        {
            GameObject[] weapons = inventory.GetWeapons();
            //get the weapon slot
            GameObject weaponslot = selectedWeapon == 0 ? inventory.weaponSlot1 : inventory.weaponSlot2;
            GameObject otherslot = selectedWeapon == 0 ? inventory.weaponSlot2 : inventory.weaponSlot1;
            //check if any weapon is equipped
            if (equippedWeapon == null)
            {
                anim.SetTrigger("Equip");
                inventory.ParentToSlot(weapons[selectedWeapon], inventory.equippedWeaponSlot);
                SetEquippedWeapon(weapons[selectedWeapon]);
            }
            //weapon already equipped
            else
            {
				//change cams
                if (gameObject.GetComponent<AIWeaponManager>() == null)
    				camManager.ChangeWeapon();
                //Equipped weapon to be put away
                if (weapons[selectedWeapon] == equippedWeapon)
                {
                    anim.SetTrigger("Unequip");
                    SetEquippedWeapon(null);
                    inventory.ParentToSlot(weapons[selectedWeapon], weaponslot);
                    ik.ikActive = false;
                }
                //Swapping weapons
                else
                {
                    anim.SetTrigger("Equip");
                    inventory.ParentToSlot(equippedWeapon, otherslot);
                    SetEquippedWeapon(null);
                    ik.ikActive = false;
                    inventory.ParentToSlot(weapons[selectedWeapon], inventory.equippedWeaponSlot);
                }
            }
        }

        public int GetSelectedWeapon()
        {
            return selectedWeapon;
        }

        public GameObject GetEquippedWeapon()
        {
            return equippedWeapon;
        }

        public virtual void SetEquippedWeapon(GameObject weapon)
        {
            equippedWeapon = weapon;
            if(weapon == null)
                anim.SetBool("GunEquipped", false);
        }
			
    }
}
