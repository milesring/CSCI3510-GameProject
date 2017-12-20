using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
	private Object[] weapons;
	private Object[] items;
	private List<Object> allItems;
	private GameObject[] locations;
	private GameObject itemsObject;
	private int spawnedWeapons;
	private CharacterSpawner charSpawner;
	GameObject m4Ammo = null;
	GameObject akAmmo = null;
	GameObject sniperAmmo = null;

	public void Spawn () {
		charSpawner = GameObject.Find ("CharacterSpawner").GetComponent<CharacterSpawner>();
		spawnedWeapons = 0;
		itemsObject = GameObject.Find ("Items");
		weapons = Resources.LoadAll ("Prefabs/Weapons");
		items = Resources.LoadAll ("Prefabs/Items");
		allItems = new List<Object>();
		AddToList (weapons);
		AddToList (items);

	
		foreach (Object ob in allItems) {
			if (ob.name.Contains ("AmmoBox_AK")) {
				akAmmo = (GameObject)ob;
			}
			if (ob.name.Contains ("AmmoBox_M4")) {
				m4Ammo = (GameObject)ob;
			}
			if (ob.name.Contains ("AmmoBox_Sniper")) {
				sniperAmmo = (GameObject)ob;
			}
		}


		locations = GameObject.FindGameObjectsWithTag ("Spawnable");

		foreach (GameObject loc in locations) {
			Vector3 location = loc.transform.position;

			//number of items based on size of thing being spawned on
			int iterations = (int)Mathf.Floor ((loc.transform.localScale.x * loc.transform.localScale.z) / 50);

			//everything is considered at least once
			if (iterations == 0) {
				iterations = 1;
			}

			for (int i = 0; i < iterations; ++i) {
				int temp = Random.Range (0, allItems.Count);
				if (temp >= allItems.Count) {
					continue;
				}
				float randX = Random.Range ((-loc.transform.localScale.x / 2) + location.x, (loc.transform.localScale.x / 2) + location.x);
				float randZ = Random.Range ((-loc.transform.localScale.z / 2) + location.z, (loc.transform.localScale.z / 2) + location.z);
				GameObject tempItem = (GameObject)Instantiate (
					                     allItems [temp], new Vector3 (randX, location.y + loc.transform.localScale.y, randZ), Quaternion.Euler (90, 0, 0));

				//if item is a gun, spawn one box of ammo nearby
				if(tempItem.CompareTag("Gun")){
					GameObject tempAmmo = null;
					switch (tempItem.name) {
					case "AK-47":
					case "AK-47(Clone)":
						tempAmmo = akAmmo;
					break;
					case "M4_Carbine":
					case "M4_Carbine(Clone)":
						tempAmmo = m4Ammo;
					break;
					case "L96_Sniper_Rifle":
					case "L96_Sniper_Rifle(Clone)":
						tempAmmo = sniperAmmo;
					break;
				default:
					break;
				}

					GameObject tempAmmoItem = Instantiate(tempAmmo, new Vector3(randX+Random.Range(-2f, 2f), tempItem.transform.position.y, randZ+Random.Range(-2f, 2f)), Quaternion.identity);
					tempAmmoItem.transform.parent = itemsObject.transform;
					spawnedWeapons++;
				}

				tempItem.transform.parent = itemsObject.transform;
			}

		}
		while (spawnedWeapons < charSpawner.enemyCount) {
			SpawnWeapon ();
		}
	}

	void SpawnWeapon(){
		GameObject loc = locations[Random.Range(0, locations.Length-1)];
		Vector3 location = loc.transform.position;

		float randX = Random.Range ((-loc.transform.localScale.x / 2) + location.x, (loc.transform.localScale.x / 2) + location.x);
		float randZ = Random.Range ((-loc.transform.localScale.z / 2) + location.z, (loc.transform.localScale.z / 2) + location.z);

		GameObject tempItem = (GameObject)Instantiate (
			weapons [Random.Range (0, weapons.Length - 1)], new Vector3 (randX, location.y + loc.transform.localScale.y, randZ), Quaternion.Euler (90, 0, 0));

		if (tempItem.name.Contains ("Grenade")) {
			Destroy (tempItem);
			return;
		}
		//if item is a gun, spawn one box of ammo nearby
		if(tempItem.CompareTag("Gun")){
			GameObject tempAmmo = null;
			switch (tempItem.name) {
			case "AK-47":
			case "AK-47(Clone)":
				tempAmmo = akAmmo;
				break;
			case "M4_Carbine":
			case "M4_Carbine(Clone)":
				tempAmmo = m4Ammo;
				break;
			case "L96_Sniper_Rifle":
			case "L96_Sniper_Rifle(Clone)":
				tempAmmo = sniperAmmo;
				break;
			default:
				break;
			}

			GameObject tempAmmoItem = Instantiate(tempAmmo, new Vector3(randX+Random.Range(-2f, 2f), tempItem.transform.position.y, randZ+Random.Range(-2f, 2f)), Quaternion.identity);
			tempAmmoItem.transform.parent = itemsObject.transform;

		}

		tempItem.transform.parent = itemsObject.transform;
		spawnedWeapons++;
	}
	
	void AddToList(Object[] items){
		foreach(Object item in items){
			allItems.Add(item);
		}
	}
}
