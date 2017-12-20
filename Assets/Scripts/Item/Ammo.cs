using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour {

	public enum AmmoType{
		M4, 
		AK, 
		Sniper
	};
	public int ammoAmount;
	public AmmoType ammoType;

    public int GetAmmoType() {
        return (int)ammoType;
    }
}
