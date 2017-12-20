using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Player;
using System;
using System.IO;
//using UnityEditor;

namespace Assets.Scripts.UI
{
    public class InventoryUI : MonoBehaviour
    {
        private bool locked;
        public GameObject inventoryUI;
        private Inventory inventory;
        private WeaponManager weaponManager;
        private List<GameObject> inventoryItems;
        private List<GameObject> vicinityItems;
        public GameObject inventoryContentPane;
        public GameObject vicinityContentPane;
        public GameObject itemPrefab;
        private List<Sprite> itemIcons;
        public Image invArmor;
        public Image invBackpack;
        public Image invHelmet;
        private Sprite invArmorEmpty;
        private Sprite invBackpackEmpty;
        private Sprite invHelmetEmpty;
        public GameObject PickupText;
        public Slider backpackHUD, backpackInv;
        public Image weaponSlot1;
        public Image weaponSlot2;
        private List<Sprite> weaponSlot;
        public Image M4_HUD;
        public Image AK_HUD;
        public Image L96_HUD;
        private List<Sprite> M4_HUD_Opt;
        private List<Sprite> AK_HUD_Opt;
        private List<Sprite> L96_HUD_Opt;
        private Camera cam;
        // Use this for initialization
        // Load all the resources into variables at the start
        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            locked = true;
            cam = Camera.main;
			inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
			weaponManager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
            inventoryItems = new List<GameObject>();
            vicinityItems = new List<GameObject>();
            itemIcons = new List<Sprite>(21);
            InitializeItemIcons();
            invArmorEmpty = LoadNewSprite("Assets/Resources/Sprites/inventory-armor-empty.png");
            invBackpackEmpty = LoadNewSprite("Assets/Resources/Sprites/inventory-backpack-empty.png");
            invHelmetEmpty = LoadNewSprite("Assets/Resources/Sprites/inventory-helmet-empty.png");
            weaponSlot = new List<Sprite>(4);
            weaponSlot.Add(LoadNewSprite("Assets/Resources/Sprites/inventory-gun-empty.png"));
            weaponSlot.Add(LoadNewSprite("Assets/Resources/Sprites/M4_invSlot.png"));
            weaponSlot.Add(LoadNewSprite("Assets/Resources/Sprites/AK_invSlot.png"));
            weaponSlot.Add(LoadNewSprite("Assets/Resources/Sprites/L96_invSlot.png"));
            M4_HUD_Opt = new List<Sprite>(2);
            M4_HUD_Opt.Add(LoadNewSprite("Assets/Resources/Sprites/m4-60.png"));
            M4_HUD_Opt.Add(LoadNewSprite("Assets/Resources/Sprites/m4-full.png"));
            AK_HUD_Opt = new List<Sprite>(2);
            AK_HUD_Opt.Add(LoadNewSprite("Assets/Resources/Sprites/ak-60.png"));
            AK_HUD_Opt.Add(LoadNewSprite("Assets/Resources/Sprites/ak-full.png"));
            L96_HUD_Opt = new List<Sprite>(2);
            L96_HUD_Opt.Add(LoadNewSprite("Assets/Resources/Sprites/sniper-60.png"));
            L96_HUD_Opt.Add(LoadNewSprite("Assets/Resources/Sprites/sniper-full.png"));
        }

        private void InitializeItemIcons()
        {
            ItemProperties.Items i;
            for (i = ItemProperties.Items.GrenadeIcon; i <= ItemProperties.Items.VestIcon; i++)
                itemIcons.Add(LoadNewSprite("Assets/Resources/Sprites/" + i.ToString() + ".png"));
        }

        // Update is called once per frame
        void Update()
        {
            //Lock/Hide & Unlock/Show mouse && Show/Hide Inventory
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                locked = !locked;
                inventoryUI.SetActive(!inventoryUI.activeSelf);
                Cursor.visible = !Cursor.visible;
                if (locked)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    UpdateInventoryItems();
                }
            }
            //Display Pickup Text
            CheckPickup();

            UpdateVicinityItems();
            UpdateEquipment();
            backpackHUD.value = (float)inventory.GetCurrentInventorySize() / (float)inventory.GetMaxInventorySize();
            backpackInv.value = (float)inventory.GetCurrentInventorySize() / (float)inventory.GetMaxInventorySize();
            //ResetHUDWeapons();
            //UpdateWeaponImage(inventory.weaponSlot1);
            //UpdateWeaponImage(inventory.weaponSlot2);
           // UpdateWeaponImage(inventory.equippedWeaponSlot);
        }

        private void UpdateVicinityItems()
        {
            foreach (GameObject item in vicinityItems)
                Destroy(item);
            vicinityItems.Clear();
            RectTransform rect;
            int i = 1;
            Collider[] colliders = Physics.OverlapSphere(inventory.transform.position, 3);
            foreach (Collider nearbyObject in colliders)
            {
                string tag = nearbyObject.gameObject.tag;
                // Debug.Log(tag);
                foreach (string t in Inventory.GetItemTagList())
                {
                    if (tag.Contains(t))
                    {
                        rect = vicinityContentPane.transform as RectTransform;
                        GameObject item = Instantiate(itemPrefab, rect);
                        item.transform.SetParent(vicinityContentPane.transform);
                        item.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemIcons[(int)nearbyObject.GetComponent<ItemProperties>().itemIcon];
                        item.transform.Find("ItemName").GetComponent<Text>().text = nearbyObject.GetComponent<ItemProperties>().itemName;
                        item.transform.Find("ItemDescription").GetComponent<Text>().text = nearbyObject.GetComponent<ItemProperties>().itemDescription;
                        item.transform.localPosition = new Vector3(item.transform.localPosition.x, inventoryContentPane.transform.localPosition.y - i * ((RectTransform)item.transform).rect.height, item.transform.localPosition.z);
                        vicinityItems.Add(item);
                        i++;
                    }
                }

            }
        }

        private void UpdateInventoryItems()
        {
            foreach (GameObject item in inventoryItems)
                Destroy(item);
            inventoryItems.Clear();
            //inventoryContentPane.transform.localScale = new Vector3(1,)
            RectTransform rect;
            int i;
            //(inventoryContentPane.transform as RectTransform).sizeDelta = new Vector2((inventoryContentPane.transform as RectTransform).sizeDelta.x, (inventoryContentPane.transform as RectTransform).sizeDelta.y * 2);
            for(i=0;i<inventory.GetInventory().Count;i++)
            {
                rect = inventoryContentPane.transform as RectTransform;

                GameObject item = Instantiate(itemPrefab, rect);
                item.transform.SetParent(inventoryContentPane.transform);
                item.transform.Find("ItemIcon").GetComponent<Image>().sprite = itemIcons[(int)inventory.GetInventory()[i].GetComponent<ItemProperties>().itemIcon];
                item.transform.Find("ItemName").GetComponent<Text>().text = inventory.GetInventory()[i].GetComponent<ItemProperties>().itemName;
                item.transform.Find("ItemDescription").GetComponent<Text>().text = inventory.GetInventory()[i].GetComponent<ItemProperties>().itemDescription;
                item.transform.localPosition = new Vector3(item.transform.localPosition.x, inventoryContentPane.transform.localPosition.y - (i+1)*((RectTransform)item.transform).rect.height, item.transform.localPosition.z);
                inventoryItems.Add(item);
            }
        }

        private void CheckPickup()
        {
            RaycastHit reach;
            if (Physics.SphereCast(cam.transform.position, 0.2f, cam.transform.forward, out reach, 3.0f, inventory.GetMask()))
            {
                if (reach.transform != null)
                {
                    GameObject item = reach.collider.gameObject;
                    foreach (string tag in Inventory.GetItemTagList())
                    {
                        if (item.CompareTag(tag) )
                        {
                            if (item.GetComponent<ItemProperties>().slotSize + inventory.GetCurrentInventorySize() > inventory.GetMaxInventorySize())
                                PickupText.GetComponent<Text>().text = "Not Enough Space!";
                            else
                            {
                                PickupText.GetComponent<Text>().text = "Pick up " + item.GetComponent<ItemProperties>().itemName;
                                return;
                            }
                        }
                    }
                }
            }
            PickupText.GetComponent<Text>().text = "";
        }

        public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
        {

            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

            Sprite NewSprite = new Sprite();
            Texture2D SpriteTexture = LoadTexture(FilePath);
            NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

            return NewSprite;
        }

        public Texture2D LoadTexture(string FilePath)
        {

            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
                if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                    return Tex2D;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }

        private void UpdateEquipment()
        {
//			if (inventory.helmetSlot != null) {
//				invHelmet.sprite = itemIcons[(int)(inventory.helmetSlot.GetComponent<ItemProperties>().itemIcon)];
//			} else {
//				invHelmet.sprite = invHelmetEmpty;
//			}
//            if (inventory.vestSlot != null)
//                invArmor.sprite = itemIcons[(int)(inventory.vestSlot.GetComponent<ItemProperties>().itemIcon)];
//            else
//                invArmor.sprite = invArmorEmpty;
//            if (inventory.backpackSlot != null)
//                invBackpack.sprite = itemIcons[(int)(inventory.backpackSlot.GetComponent<ItemProperties>().itemIcon)];
//            else
//                invBackpack.sprite = invBackpackEmpty;
        }

        private void ResetHUDWeapons()
        {
           // weaponSlot1.sprite = weaponSlot[0];
           // weaponSlot2.sprite = weaponSlot[0];
            M4_HUD.color = new Color(0, 0, 0, 0);
            AK_HUD.color = new Color(0, 0, 0, 0);
            L96_HUD.color = new Color(0, 0, 0, 0);
        }

        void UpdateWeaponImage(GameObject weaponSlot)
        {
            if (weaponSlot.transform.Find("M4_Carbine") || weaponSlot.transform.Find("M4_Carbine(Clone)"))
            {
                if (weaponSlot == inventory.equippedWeaponSlot)
                {
                    M4_HUD.sprite = M4_HUD_Opt[1];
                    if (weaponManager.GetSelectedWeapon() == 0)
                        weaponSlot1.sprite = this.weaponSlot[1];
                    else
                        weaponSlot2.sprite = this.weaponSlot[1];
                }
                else
                {
                    M4_HUD.sprite = M4_HUD_Opt[0];
                    if (weaponSlot == inventory.weaponSlot1)
                        weaponSlot1.sprite = this.weaponSlot[1];
                    else
                        weaponSlot2.sprite = this.weaponSlot[1];
                }
                M4_HUD.color = new Color(0, 0, 0, 1);
            }
            else if (weaponSlot.transform.Find("AK-47") || weaponSlot.transform.Find("AK-47(Clone)"))
            {
                if (weaponSlot == inventory.equippedWeaponSlot)
                {
                    AK_HUD.sprite = AK_HUD_Opt[1];
                    if (weaponManager.GetSelectedWeapon() == 0)
                        weaponSlot1.sprite = this.weaponSlot[2];
                    else
                        weaponSlot2.sprite = this.weaponSlot[2];
                }
                else
                {
                    AK_HUD.sprite = AK_HUD_Opt[0];
                    if (weaponSlot == inventory.weaponSlot1)
                        weaponSlot1.sprite = this.weaponSlot[2];
                    else
                        weaponSlot2.sprite = this.weaponSlot[2];
                }
                AK_HUD.color = new Color(0, 0, 0, 1);
            }
            else if (weaponSlot.transform.Find("L96_Sniper_Rifle") || weaponSlot.transform.Find("L96_Sniper_Rifle(Clone)"))
            {
                if (weaponSlot == inventory.equippedWeaponSlot)
                {
                    L96_HUD.sprite = L96_HUD_Opt[1];
                    if (weaponManager.GetSelectedWeapon() == 0)
                        weaponSlot1.sprite = this.weaponSlot[3];
                    else
                        weaponSlot2.sprite = this.weaponSlot[3];
                }
                else
                {
                    L96_HUD.sprite = L96_HUD_Opt[0];
                    if (weaponSlot == inventory.weaponSlot1)
                        weaponSlot1.sprite = this.weaponSlot[3];
                    else
                        weaponSlot2.sprite = this.weaponSlot[3];
                }
                L96_HUD.color = new Color(0, 0, 0, 1);
            }

        }
    }
}