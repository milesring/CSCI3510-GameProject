using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerInventory : Inventory {

	Camera cam;
	CameraManager camManager;

	void Start () {
        base.Initialize();
        cam = Camera.main;
		camManager = cam.GetComponent<CameraManager> ();
		//used for ignoring the player when raycasting in 3rd person
        mask = 1 << 8;
        mask = ~mask;

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.F)) {
			PickupItem ();
			//Debug.Log ("Inventory size: " + currentInventorySize + "/" + maxInventorySize);
		}
        /*
		if (Input.GetKeyDown (KeyCode.B)) {
			DropItem (0);
			//Debug.Log ("Inventory size: " + currentInventorySize + "/" + maxInventorySize);
		}*/
		/*
		if (Input.GetKeyDown (KeyCode.G)) {
			DropWeapon (weaponManager.GetSelectedWeapon ());
		}
		*/

		if (Input.GetKeyDown (KeyCode.T)) {
			UseMeds ();
		}
			
	}

	void PickupItem(){
		RaycastHit reach;
		if (Physics.SphereCast (cam.transform.position, 0.2f, cam.transform.forward, out reach, 3.0f, mask)){
        //if (Physics.Raycast(cam.transform.position, cam.transform.forward, out reach, 4.0f, mask)){
			//Debug.Log (reach.transform.name);

			if (reach.transform != null) {
				GameObject item = reach.collider.gameObject;

                ItemProperties properties = item.GetComponent<ItemProperties>();
				if (properties != null && properties.slotSize + GetCurrentInventorySize() <= GetMaxInventorySize()) {
					if (base.weaponManager.GetEquippedWeapon () != null)
                        StartCoroutine(DisableLeftHandIK());
                    base.anim.SetTrigger("Pickup");
					camManager.Pickup ();
					AddItem(item);
				}
				if (item.name.Contains ("Helmet")) {
					camManager.CullHelm ();
				}

			}
		}
	}

}
