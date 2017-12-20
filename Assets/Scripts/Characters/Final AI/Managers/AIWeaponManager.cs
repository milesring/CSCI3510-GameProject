using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Player;

public class AIWeaponManager : WeaponManager {

    new AIInventoryManager inventory;

    protected override void Initilize() {
        aim.transform.position = playerHead.transform.position;
        aim.transform.rotation = playerHead.transform.rotation;
        aim.transform.SetParent(playerHead.transform);

        inventory = GetComponent<AIInventoryManager>();
        base.inventory = inventory;
    }

    protected override void Update() {

    }

    protected override void FixedUpdate() {
        UpdateIK();
    }

    void UpdateGunFace() { /*
        if (equippedWeapon) {
            inventory.equippedWeaponSlot.transform.LookAt(lookDir.transform);
            //inventory.equippedWeaponSlot.transform.Rotate(90, 0, 0);
            equippedWeapon.transform.LookAt(lookDir.transform);
            equippedWeapon.transform.Rotate(0, 90, 0);
        } */
    }

    public void FireAt(GameObject target) {
        AIThreat threat = target.GetComponent<AIThreat>();
        if (threat != null)
            target = threat.head;
        aim.transform.LookAt(target.transform);

        Debug.DrawRay(aim.transform.position, aim.transform.forward, Color.red, 10, true);

        if (equippedWeapon != null) {
            /*         if (Input.GetMouseButton(0)) {
                           Shoot();
                       } else
                           anim.SetBool("Shooting", false); */
            Shoot();
        } else {
            Punch();
        }
    }

    protected void SetAimParent(GameObject parent) {
        aim.transform.position = parent.transform.position;
        aim.transform.rotation = parent.transform.rotation;
        aim.transform.SetParent(parent.transform);
    }

    public void WeaponPickedUp() {
        if (equippedWeapon == null) {
            selectedWeapon = 0;
            SelectWeapon();
        }
    }

    public override void SetEquippedWeapon(GameObject weapon) {
        base.SetEquippedWeapon(weapon);
        if (equippedWeapon != null) {
            equippedWeapon.transform.localRotation = Quaternion.identity;
            equippedWeapon.transform.Rotate(0, 90, 0);
        }
    }

    public GameObject GetAimObject() {
        return aim;
    }

    public bool SetWeaponForRangeSqr(float sqrRange) {
        GameObject[] weapons = inventory.GetWeapons();
        if (weapons[0] != null) {
            Gun gun = weapons[0].GetComponent<Gun>();
            if (Mathf.Pow(gun.range, 2) > sqrRange && gun.GetCurrentAmmo() > 0) {
                if (selectedWeapon != 0) {
                    selectedWeapon = 0;
                    SelectWeapon();
}
                return true;
            }
        } else if (weapons[1] != null) {
            Gun gun = weapons[1].GetComponent<Gun>();
            if (Mathf.Pow(gun.range, 2) > sqrRange && gun.GetCurrentAmmo() > 0) {
                if (selectedWeapon != 1) {
                    selectedWeapon = 1;
                    SelectWeapon();
                }
                return true;
            }
        }
        return false;
    }

    public int HasGunOfType(int type) {
        GameObject[] weapons = inventory.GetWeapons();
        Gun gun;
        if (weapons[0] != null) {
            gun = weapons[0].GetComponent<Gun>();
            if (gun.GetAmmoType() == type)
                return 0;
        } else if (weapons[1] != null) {
            gun = weapons[1].GetComponent<Gun>();
            if (gun.GetAmmoType() == type)
                return 1;
        }
        return -1;
    }

    public bool IsReloadingWeapon() {
        if (equippedWeapon != null) {
            Gun gun = equippedWeapon.GetComponent<Gun>();
            return gun.GetReloading();
        }
        return false;
    }
}
