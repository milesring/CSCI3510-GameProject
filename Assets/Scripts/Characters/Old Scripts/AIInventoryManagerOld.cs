/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIWeaponManager))]

public class AIInventoryManagerOld : Inventory {

    AITriggerNode triggerNode;
    AITriggerNode grabReach;

    AIMovementController movement;
    AdvancedAI ai;

    // Use this for initialization
    void Start () {
        base.Initialize();

        triggerNode = gameObject.AddComponent<AITriggerNode>();
        triggerNode.SetRadius(3);
        triggerNode.SetPosition(new Vector3(0, 0.5f, 0));
        triggerNode.triggeredFunc += Triggered;
        triggerNode.stayFunc += ItemStay;
        triggerNode.SetName("Inventory Manager");

        grabReach = gameObject.AddComponent<AITriggerNode>();
        grabReach.SetRadius(0.1f);
        grabReach.SetPosition(new Vector3(0, 1, 0));
        grabReach.stayFunc += GrabTriggered;
        grabReach.SetName("Grab Reach");

        movement = GetComponent<AIMovementController>();
        ai = GetComponent<AdvancedAI>();
    }

    public void Triggered(Collider other)
    {
        GameObject item = other.gameObject;
        if (CheckTagInItemList(other.tag) && CheckItemForOwner(item) && ai.goal == AdvancedAI.Goals.LOOT)
            movement.AddTarget(item);
    }

    public void ItemStay(Collider other)
    {
        if (CheckTagInItemList(other.tag) && CheckItemForOwner(other.gameObject))
            movement.RemoveTarget(other.gameObject);
    }

    public void GrabTriggered(Collider other)
    {
        if (CheckTagInItemList(other.tag) && CheckItemForOwner(other.gameObject) && ai.goal == AdvancedAI.Goals.LOOT)
            WeighItemValue(other.gameObject);
        movement.RemoveTarget(other.gameObject);
    }

    public bool CheckItemForOwner(GameObject item)
    {
        Gun gun = item.GetComponent<Gun>();
        ItemProperties itemProp = item.GetComponent<ItemProperties>();
        return (gun != null && !gun.owned) || (itemProp != null && !itemProp.owned);
    }

    public void WeighItemValue(GameObject item)
    {
        // TODO
        PickupItem(item);

    }

    void PickupItem(GameObject item)
    {        /*
        // RAYCAST TO SEE IF ITEM IS VISIBLE
        RaycastHit reach;
        Transform sight = new GameObject().transform;
        sight.position = gameObject.transform.position;
        sight.LookAt(item.transform);

        if (Physics.Raycast(sight.position, sight.forward, out reach, 4.0f) {
            //Debug.Log (reach.transform.name);

            if (reach.transform != null)
            {
                GameObject touchedObject = reach.collider.gameObject;

                if (touchedObject == item)
                {
*///                */
/*
        AddItem(item);
        if (item.GetComponent<Gun>() != null)
        {
            weaponManager.SetEquippedWeapon(item);
        }
                    /*
                }
            }
        }
*///        */
/*
	}
}
*/