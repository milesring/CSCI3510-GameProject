using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInventoryManager : Inventory {

    EnemyAI ai;
    AIMovementManager movement;
    new AIWeaponManager weaponManager;

    List<GameObject> desiredItems;

	// Use this for initialization
	void Start () {
        base.Initialize();

        ai = GetComponent<EnemyAI>();
        movement = GetComponent<AIMovementManager>();
        weaponManager = GetComponent<AIWeaponManager>();
        base.weaponManager = weaponManager;

        desiredItems = new List<GameObject>();

        AITriggerManager triggers = GetComponent<AITriggerManager>();
        triggers.AddTrigger("Item View Distance", 4, ItemTriggered);
        triggers.AddTrigger("Item Grab Distance", 1.5f, GrabTriggered);
    }

    public void ItemTriggered(Collider other) {
        GameObject item = other.gameObject;
        ItemProperties itemProp = item.GetComponent<ItemProperties>();
        if (itemProp != null && !itemProp.owned && WeighItemValue(item) && ai.goal != EnemyAI.Goals.FLEE && ai.goal != EnemyAI.Goals.HIDE) {
            AddItemToDesiredList(item);
        }
    }

    public void GrabTriggered(Collider other) {
        if (other.gameObject.GetComponent<ItemProperties>() != null && desiredItems.Contains(other.gameObject))
            PickupItem(other.gameObject);
    }

    bool WeighItemValue(GameObject item) {
        ItemProperties properties = item.GetComponentInParent<ItemProperties>();
        Gun gun = item.GetComponent<Gun>();

        if (gun != null) {
            if (weapons[0] != null && weapons[1] != null) // already has 2 weapons
                return false;
            if ((weapons[0] == null || (weapons[0].GetComponent<Gun>().GetAmmoType() != gun.GetAmmoType())) &&
                (weapons[1] == null || (weapons[1].GetComponent<Gun>().GetAmmoType() != gun.GetAmmoType()))) // each gun is either null or not the same as the one in question
                return true;
        } else {
            if (item.CompareTag("Equipment")) {
                GameObject equipment;
                if (GetCurrentVest() == null)
                    return true;
                else if (GetCurrentHelmet() == null)
                    return true;
                else if ((equipment = GetCurrentBackpack()) == null || (properties.inventoryIncrease > equipment.GetComponent<ItemProperties>().inventoryIncrease))
                        return true;
            } else if (item.CompareTag("Ammo")) {
                foreach (GameObject weapon in weapons) {
                    if (weapon != null && weapon.GetComponent<Gun>().GetAmmoType() == item.GetComponent<Ammo>().GetAmmoType())
                        return true;
                }
                return false;

            } else if (item.CompareTag("Item")) {
                if (GetCurrentInventorySize() + properties.slotSize < GetMaxInventorySize())
                    return true;
            }
        }




        return false;
    }

    void AddItemToDesiredList(GameObject item) {
        ItemProperties properties = item.GetComponent<ItemProperties>();
        if (properties == null || desiredItems.Contains(item))
            return;

        movement.AddTarget(item);
        desiredItems.Add(item);

        properties.AddListener(ItemWasCollected);
    }

    void RemoveItemFromDesiredList(GameObject item) {
        ItemProperties properties = item.GetComponent<ItemProperties>();
        if (properties == null || !desiredItems.Contains(item))
            return;

        desiredItems.Remove(item);
        movement.RemoveTarget(item);

        properties.RemoveListener(ItemWasCollected);
    }

    void ItemWasCollected(ItemProperties item) {
        RemoveItemFromDesiredList(item.gameObject);
    }

    void PickupItem(GameObject item) {
        ItemProperties properties = item.GetComponent<ItemProperties>();
        if (properties != null && properties.slotSize + GetCurrentInventorySize() <= GetMaxInventorySize()) {
            if (base.weaponManager.GetEquippedWeapon() != null)
                StartCoroutine(DisableLeftHandIK());
            StartCoroutine(movement.PauseFor(1f));
            base.anim.SetTrigger("Pickup");
//            camManager.Pickup();
            AddItem(item);
            RemoveItemFromDesiredList(item);
            if (item.GetComponent<Gun>() != null)
                ai.lootLevel += 5;
            else if (item.CompareTag("Ammo")) {
                int gunID = weaponManager.HasGunOfType((int)item.GetComponent<Ammo>().ammoType);
                if (gunID != -1)
                    StartCoroutine(ReloadWeaponWithID(gunID));
                ai.lootLevel += 2;
            } else {
                ai.lootLevel++;
            }
        }
    }

    IEnumerator ReloadWeaponWithID(int gunID) {
        Gun gun = weapons[gunID].GetComponent<Gun>();
        if (gun.GetCurrentAmmo() == 0) {
            if (weaponManager.selectedWeapon != gunID || weaponManager.GetEquippedWeapon() == null) {
                Debug.Log("equip gun");
                weaponManager.selectedWeapon = gunID;
                weaponManager.SelectWeapon();
                yield return null;
            }
            weaponManager.ReloadWeapon();
        }
    }

    public void UseMedical() {
        UseMeds();
    }

}
