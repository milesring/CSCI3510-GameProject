using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemProperties : MonoBehaviour {
    private delegate void ItemPickup(ItemProperties item);
    private event ItemPickup Pickup;

    private bool _owned = false;
    public bool owned {
        get { return _owned; }
        set {
            _owned = value;
            if (value == true && Pickup != null)
                Pickup(this);
        }
    }
    public enum Items {GrenadeIcon, M4_Icon, AK_Icon, L96_Icon, AmmoBoxAKIcon, AmmoBoxM4Icon, AmmoBoxL96Icon, AmmoCrateAKIcon,
        AmmoCrateM4Icon, AmmoCrateL96Icon, BandagesIcon, FirstAidIcon, MedkitIcon, LightBackpack_1Icon, LightBackpack_2Icon,
        MediumBackpack_1Icon, MediumBackpack_2Icon, MilitaryBackpackIcon, HelmetIcon, HelmetVisorUpIcon, HelmetVisorDownIcon, VestIcon};

	public int slotSize;
	public int inventoryIncrease;
    public string itemName;
    public string itemDescription;
    public Items itemIcon;
    //public image icon;

    public void AddListener(Action<ItemProperties> listener) {
        Pickup += new ItemPickup(listener);
    }

    public void RemoveListener(Action<ItemProperties> listener) {
        Pickup -= new ItemPickup(listener);
    }
}
