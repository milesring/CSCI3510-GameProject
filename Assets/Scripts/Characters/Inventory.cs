using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Player;

public class Inventory : MonoBehaviour {

	// Class that both PlayerInventory and AIInventory inherit from

	public GameObject helmetSlot, backpackSlot, vestSlot; 
	public GameObject weaponSlot1, weaponSlot2, equippedWeaponSlot;
	protected int mask;

	protected List<GameObject> inventory;
	protected GameObject[] weapons;
	protected WeaponManager weaponManager;
	IKControl ik;
	protected Animator anim;
	int[] ammo;
	Status characterStatus;
	int maxInventorySize;
	int itemInventorySize;
	int currentInventorySize;
	int ammoInventorySize;

    public bool pickup;

    // Use this for initialization
    protected void Initialize () {
        pickup = false;
		inventory = new List<GameObject>();
		maxInventorySize = 10;
		ammoInventorySize = 0;
		itemInventorySize = 0;
		currentInventorySize = 0;
		weapons = new GameObject[2];
		ik = GetComponent<IKControl>();
		anim = GetComponent<Animator>();
		weaponManager = GetComponent<WeaponManager> ();
        characterStatus = gameObject.GetComponent<Status>();

		//used for ignoring the character when raycasting in 3rd person
		// TODO
		mask = 1 << 8;
		mask = ~mask;

		//change to how many weapons in game
		ammo = new int[4];

        // simply creating an empty game object to use for the slot objects rather than the Mixamo body parts
        // prefered to do it in code rather than on the prefabs
        GameObject helmetTemp = new GameObject();
        helmetTemp.transform.position = helmetSlot.transform.position;
        helmetTemp.transform.rotation = helmetSlot.transform.rotation;
        helmetTemp.transform.SetParent(helmetSlot.transform);
        helmetSlot = helmetTemp;
        helmetSlot.name = "Helmet Slot";

        GameObject backpackTemp = new GameObject();
        backpackTemp.transform.position = backpackSlot.transform.position;
        backpackTemp.transform.rotation = backpackSlot.transform.rotation;
        backpackTemp.transform.SetParent(backpackSlot.transform);
        backpackSlot = backpackTemp;
        backpackSlot.name = "Backpack Slot";

        GameObject vestTemp = new GameObject();
        vestTemp.transform.position = vestSlot.transform.position;
        vestTemp.transform.rotation = vestSlot.transform.rotation;
        vestTemp.transform.SetParent(vestSlot.transform);
        vestSlot = vestTemp;
        vestSlot.name = "Vest Slot";
    }


    /*************************/
    /* Adding/Dropping items */
    /*************************/

    public void AddItem(GameObject item){

		if (item.CompareTag ("Gun") && !item.GetComponent<ItemProperties>().owned) {
            item.GetComponent<ItemProperties>().owned = true;

            //check for open slot
            item.layer = 8;
			item.GetComponent<Gun>().StopSound();

			if (weapons [0] == null) {
				weapons [0] = item;
				//put on back
				ParentToSlot(item, weaponSlot1);
			} else if (weapons [1] == null) {
				weapons [1] = item;
				//put on back
				ParentToSlot(item, weaponSlot2);
			} else {
				//no slots open, drop currently equipped
				int current = weaponManager.GetSelectedWeapon ();
				DropWeapon (current);
				weapons [current] = item;
				weaponManager.SetEquippedWeapon (weapons [current]);
				ParentToSlot(item, equippedWeaponSlot);
				ik.ikActive = true;
			}

			Rigidbody rb = item.GetComponent<Rigidbody> ();
			rb.isKinematic = true;
			rb.useGravity = false;

            BoxCollider bc = item.GetComponent<BoxCollider>();
            if (bc) {
                bc.enabled = false;
            }

            SphereCollider sc = item.GetComponent<SphereCollider>();
            if (sc) {
                sc.enabled = false;
            }
        }

        //item
        else if (item.CompareTag ("Item") && !item.GetComponent<ItemProperties>().owned) {
            item.GetComponent<ItemProperties>().owned = true;

            if (currentInventorySize + item.GetComponent<ItemProperties> ().slotSize < maxInventorySize) {
				item.transform.parent = this.transform;
				item.SetActive (false);
				inventory.Add (item);
				itemInventorySize += item.GetComponent<ItemProperties> ().slotSize;
				UpdateInventorySize ();
			}
		}

		//ammo
		else if (item.CompareTag ("Ammo") && !item.GetComponent<ItemProperties>().owned) {
            item.GetComponent<ItemProperties>().owned = true;

            item.transform.parent = this.transform;
			item.SetActive (false);

			switch (item.GetComponent<Ammo> ().ammoType) {
			case Ammo.AmmoType.M4:
				ammo [0] += item.GetComponent<Ammo> ().ammoAmount;
				break;
			case Ammo.AmmoType.AK:						
				ammo [1] += item.GetComponent<Ammo> ().ammoAmount;
				break;
			case Ammo.AmmoType.Sniper:
				ammo [2] += item.GetComponent<Ammo> ().ammoAmount;
				break;
			default:
				break;
			}
			Destroy(item);
			UpdateAmmoInInventory ();
		}

		// equipment
		else if (item.CompareTag("Equipment") && !item.GetComponent<ItemProperties>().owned)
		{
            item.GetComponent<ItemProperties>().owned = true;
			item.layer = 8;
            item.transform.parent = this.transform;
			Rigidbody rb = item.GetComponent<Rigidbody> ();
			rb.isKinematic = true;
			rb.useGravity = false;

			//increases inventory size
			maxInventorySize += item.GetComponent<ItemProperties> ().inventoryIncrease;

			//avoids collision while wearing
			BoxCollider bc = item.GetComponent<BoxCollider> ();
			if (bc) {
				bc.enabled = false;
			}

			SphereCollider sc = item.GetComponent<SphereCollider> ();
			if (sc) {
				sc.enabled = false;
			}
			EquipmentToChar (item);
		}
	}

	public void DropItem(int selectedItem) {
		if (inventory.Count == 0) {
			return;
		}

        GameObject item = inventory [selectedItem];	
		inventory.Remove (inventory [selectedItem]);
		item.transform.parent = GameObject.Find("Items").transform;
		currentInventorySize -= item.GetComponent<ItemProperties> ().slotSize;
        item.GetComponent<ItemProperties>().owned = false;

        if(item.name.Contains("Helmet") || item.name.Contains("Vest"))
        {
            characterStatus.DecreaseArmor(50);
        }

        item.SetActive (true);
		Rigidbody rb = item.GetComponent<Rigidbody>();
		BoxCollider bc = item.GetComponent<BoxCollider> ();
		if (bc) {
			bc.enabled = true;
		}

		SphereCollider sc = item.GetComponent<SphereCollider> ();
		if (sc) {
			sc.enabled = true;

			if (item.name.Contains ("Helmet")) {
				item.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
				Component[] meshRs = item.GetComponentsInChildren <MeshRenderer>();
				foreach (MeshRenderer meshRend in meshRs) {
					meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
				}
			}
		}
		item.layer = 0;	
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.AddForce( new Vector3 (0, 1, 0));
	}

	public void DropWeapon(int selectedWeapon){
		GameObject weapon = weapons[selectedWeapon];
		if (weapon != null) {
			weapons [selectedWeapon] = null;

            weapon.GetComponent<ItemProperties>().owned = false;
			weaponManager.SetEquippedWeapon (null);
			anim.SetBool("GunEquipped", false);
			weapon.GetComponent<Gun>().StopSound();
			weapon.transform.parent = GameObject.Find ("Items").transform;
			weapon.layer = 0;

			Rigidbody rb = weapon.GetComponent<Rigidbody> ();
			rb.isKinematic = false;
			rb.useGravity = true;
			rb.AddForce (new Vector3 (0, 1, 0));
			ik.ikActive = false;

            BoxCollider bc = weapon.GetComponent<BoxCollider>();
            if (bc != null) {
                bc.enabled = true;
            }

            SphereCollider sc = weapon.GetComponent<SphereCollider>();
            if (sc != null) {
                sc.enabled = true;
            }
        }
    }

    /*********************/
    /* Equip / use items */
    /*********************/

    public GameObject GetCurrentBackpack() {
        GameObject slot = backpackSlot;

        for (int i = 0; i < slot.transform.childCount; i++) {
            GameObject child = slot.transform.GetChild(i).gameObject;
            if (child.name.Contains("Backpack")) {
                return child;
            }
        }
        return null;
    }

    public GameObject GetCurrentHelmet() {
        GameObject slot = helmetSlot;

        for (int i = 0; i < slot.transform.childCount; i++) {
            GameObject child = slot.transform.GetChild(i).gameObject;
            if (child.name.Contains("Helmet")) {
                return child;
            }
        }
        return null;
    }

    public GameObject GetCurrentVest() {
        GameObject slot = vestSlot;

        for (int i = 0; i < slot.transform.childCount; i++) {
            GameObject child = slot.transform.GetChild(i).gameObject;
            if (child.name.Contains("Vest")) {
                return child;
            }
        }
        return null;
    }

    protected void DropEquipment(GameObject equipment) {
        //add to inventory then drop, done for easier item manipulation
        inventory.Add(equipment);
        DropItem(inventory.Count - 1);
    }

    public void EquipmentToChar(GameObject item){
//		GameObject slot;
        GameObject currentEquip;

		if (item.name.Contains("Backpack")) {
            /*
			slot = backpackSlot;
			for (int i = 0; i < slot.transform.childCount; i++) {
				//check for already equipped backpack
				if(slot.transform.GetChild(i).name.Contains("Backpack")){

					//add to inventory then drop, done for easier item manipulation
					inventory.Add(slot.transform.GetChild (i).gameObject);

					DropItem (inventory.Count-1);
				}

			}
            */

            //check for already equipped backpack
            currentEquip = GetCurrentBackpack();
            if (currentEquip != null) {
                DropEquipment(currentEquip);
            }

            //put on new back back
            item.transform.parent = backpackSlot.transform;
            //LightBackpack
            if(item.name.Contains("LightBackpack"))
                item.transform.localPosition = new Vector3 (-0.009f, 0.134f, -0.17f);
            //MediumBackpack
            else if (item.name.Contains("MediumBackpack"))
                item.transform.localPosition = new Vector3(-0.009f, 0.134f, -0.118f);
            //MilitaryBackpack
            else if (item.name.Contains("MilitaryBackpack"))
                item.transform.localPosition = new Vector3(-0.009f, 0.134f, -0.118f);

            item.transform.localRotation = Quaternion.Euler (14, 180, 0);
			//backpackSlot = item;

			// add new slots here
			// inventorySize += amountfromequipment
		}
		else if(item.name.Contains("Helmet")){
            /*
			slot = helmetSlot;
			for (int i = 0; i < slot.transform.childCount; i++) {
                //check for already equipped helmet
                if (slot.transform.GetChild(i).name.Contains("Helmet"))
                {
                    //add to inventory then drop, done for easier item manipulation
                    inventory.Add(slot.transform.GetChild(i).gameObject);
                    DropItem(inventory.Count - 1);
                }
			}
            */

            //check for already equipped helmet
            currentEquip = GetCurrentHelmet();
            if (currentEquip != null) {
                DropEquipment(currentEquip);
            }

            //put on new helmet
            item.transform.parent = helmetSlot.transform;
			item.transform.localPosition = new Vector3 (0f, 0.1199f, 0.0143f);
			item.transform.localRotation = Quaternion.Euler (259.7f, 0f, 0f);
            characterStatus.IncreaseArmor(50);
			//helmetSlot = item;
		}
        else if (item.name.Contains("Vest"))
        {
            /*
            slot = vestSlot;
            for (int i = 0; i < slot.transform.childCount; i++)
            {
                //check for already equipped vest
                if (slot.transform.GetChild(i).name.Contains("Vest"))
                {

                    //add to inventory then drop, done for easier item manipulation
                    inventory.Add(slot.transform.GetChild(i).gameObject);
                    DropItem(inventory.Count - 1);
                }
            }
            */

            //check for already equipped vest
            currentEquip = GetCurrentVest();
            if (currentEquip != null) {
                DropEquipment(currentEquip);
            }

            //put on new vest
            item.transform.parent = vestSlot.transform;
            item.transform.localPosition = new Vector3(0.006f, -1.185f, 0.034f);
            item.transform.localRotation = Quaternion.Euler(-2.526f, 0.877f, 0.357f);
            characterStatus.IncreaseArmor(50);
            //vestSlot = item;
        }


	}

	protected void UseMeds(){
		for (int i = 0; i < inventory.Count; i++) {
			if (inventory[i].name.Contains ("Bandages")
				|| inventory[i].name.Contains("FirstAid")
				|| inventory[i].name.Contains("Medkit")) {
                if (weaponManager.GetEquippedWeapon() != null)
                    StartCoroutine(delayMeds());
                else
                    anim.SetBool("UseMeds", true);
                //Get the healing properties
                FirstAid item = inventory [i].GetComponent<FirstAid> ();
                StartCoroutine(loopMeds(item.healTime));
                //Begin healing
                StartCoroutine(characterStatus.RestoreHealth (item.healAmount, item.healTime));
				//Remove med from bag and destroy it
				GameObject temp = inventory [i];
				itemInventorySize -= temp.GetComponent<ItemProperties> ().slotSize;
				inventory.Remove (inventory[i]);
				Destroy (temp);
			}
		}
	}

    private IEnumerator loopMeds(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetBool("UseMeds", false);
    }

    private IEnumerator delayMeds()
    {
        weaponManager.SelectWeapon();
        yield return new WaitForSeconds(0.567f);
        anim.SetBool("UseMeds", true);
    }

    public bool FindAndConsumeItem(String tag){
		foreach (GameObject item in inventory) {
			if (item.name.Contains (tag)) {
				Destroy (item, 0.5f);
				return inventory.Remove (item);
			}
		}
		return false;
	}

	public void ParentToSlot(GameObject item, GameObject weaponSlot)
	{
		item.transform.SetParent(weaponSlot.transform);
		//put weapon on back
		if (weaponSlot != equippedWeaponSlot)
		{
            StartCoroutine(delayUnequipWeapon(item));
		}
		//equip the weapon
		else
		{
            //Disable fists
            StartCoroutine(delayEquipWeapon(item));
		}
	}

    IEnumerator delayUnequipWeapon(GameObject item)
    {
        yield return new WaitForSeconds(.5f);
        item.transform.localPosition = new Vector3(0, 0, -0.2f);
        item.transform.localRotation = Quaternion.Euler(0, 90, 90);
    }

    IEnumerator delayEquipWeapon(GameObject item)
    {
        yield return new WaitForSeconds(0.75f);
        anim.SetBool("GunEquipped", true);
        //Put each gun in the correct equipped  
        switch (item.name)
        {
            case "M4_Carbine":
            case "M4_Carbine(Clone)":
                item.transform.localPosition = new Vector3(-0.029f, -0.108f, 0.269f);
                break;
            case "AK-47":
            case "AK-47(Clone)":
                item.transform.localPosition = new Vector3(-0.033f, -0.118f, 0.304f);
                break;
            case "L96_Sniper_Rifle":
            case "L96_Sniper_Rifle(Clone)":
                item.transform.localPosition = new Vector3(0.03f, -0.058f, 0.286f);
                break;
            default:
                break;
        }
        ik.ikActive = true;
        weaponManager.SetEquippedWeapon(item);
    }

	/***********/
	/* updates */
	/***********/

	public void UpdateAmmoInInventory(){
		ammoInventorySize = 0;
		for (int i = 0; i < ammo.Length; i++) {
			ammoInventorySize += ammo [i] / 20;
		}
	}

	public void UpdateInventorySize(){
		UpdateAmmoInInventory ();
		currentInventorySize = ammoInventorySize + itemInventorySize;
	}

	/***********/
	/* getters */
	/***********/

	public int GetMaxInventorySize(){
		return maxInventorySize;
	}

	public int GetCurrentInventorySize(){
		return currentInventorySize;
	}

	public GameObject[] GetWeapons(){
		return weapons;
	}

	public List<GameObject> GetInventory(){
		return inventory;
	}

	public int[] GetAmmo(){
		return ammo;
	}

    public int GetAmmoInventorySize() {
        return ammoInventorySize;
    }

    public int GetMask()
    {
        return mask;
    }

    public static string[] GetItemTagList() {
        return new string[] { "Gun", "Item", "Ammo", "Equipment" };
    }

    public static bool CheckTagInItemList(string tag) {
        foreach (string iTag in GetItemTagList())
            if (tag.Equals(iTag))
                return true;
        return false;
    }

    protected IEnumerator DisableLeftHandIK() {
        pickup = true;
        yield return new WaitForSeconds(1.2f);
        pickup = false;
    }

    public void DropAllInventory() {
        GameObject item;

        for (int i = 0; i < inventory.Count; i++)
            DropItem(i);

        if (weapons[0] != null)
            DropWeapon(0);

        if (weapons[1] != null)
            DropWeapon(1);

        item = GetCurrentBackpack();
        if (item != null)
            DropEquipment(item);

        item = GetCurrentHelmet();
        if (item != null)
            DropEquipment(item);

        item = GetCurrentVest();
        if (item != null)
            DropEquipment(item);


		GameObject[] ammoboxes = new GameObject[4];
		ammoboxes[0] = (GameObject)Resources.Load("Prefabs/Items/AmmoCrate_M4");
		ammoboxes[1] = (GameObject)Resources.Load("Prefabs/Items/AmmoCrate_AK");
		ammoboxes[2] = (GameObject)Resources.Load("Prefabs/Items/AmmoCrate_Sniper");

		for (int i = 0; i < ammo.Length; ++i) {
			if (ammo [i] > 0) {
				item = Instantiate (ammoboxes[i], transform);
				item.transform.localPosition = Vector3.zero;
				switch(i){
				case 0:
					item.GetComponent<Ammo>().ammoType = Ammo.AmmoType.M4;
					break;
				case 1:
					item.GetComponent<Ammo>().ammoType = Ammo.AmmoType.AK;
					break;
				case 2:
					item.GetComponent<Ammo>().ammoType = Ammo.AmmoType.Sniper;
					break;
				}
				item.GetComponent<Ammo>().ammoAmount = ammo[i];
				DropEquipment (item);
			}
		}
    }
}
