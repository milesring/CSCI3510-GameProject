/*
using System.Collections;
using UnityEngine;

public class AIWeaponManagerOld : MonoBehaviour
{
    public GameObject grenadePrefab;
    public AudioClip punchClip;
    GameObject lookDir;
    public int selectedWeapon = 0;
    Inventory inventory;
    GameObject equippedWeapon;
    GameObject fist;
    public GameObject playerHead;
    public GameObject aimManager;
    IKControl ik;
    Animator anim;
    AudioSource audioSource;
    int mask;
    float nextTimeToFire = 0f;
    float nextTimeToThrow = 0f;
    float grenadeThrowRate = 1f;
    float throwForce = 600f;
    bool punchSoundPlayed;

    // Use this for initialization
    void Start()
    {
        audioSource = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
        ik = GetComponent<IKControl>();
        anim = GetComponent<Animator>();
        SetEquippedWeapon(null);
        lookDir = new GameObject();
        lookDir.name = "LookDirectionObject";
        lookDir.transform.localPosition = playerHead.transform.position;
        lookDir.transform.localRotation = aimManager.transform.rotation;
        //lookDir.transform.localRotation = Quaternion.Euler(0f,0f,0f);
        lookDir.transform.Translate(0, 0, 5f);
        inventory = GetComponent<PlayerInventory>();
        equippedWeapon = null;
        fist = GameObject.FindGameObjectWithTag("rHand");
        punchSoundPlayed = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckWeaponSwitch();
        CheckFire();
        UpdateLookDirection();
    }

    void FixedUpdate()
    {
        UpdateIK();
        if (!anim.GetCurrentAnimatorStateInfo(4).IsName("Grenade"))
            CheckGrenade();
        CheckReload();
    }
    private void UpdateLookDirection()
    {
        //            lookDir.transform.localPosition = aimManager.transform.position;
        //            lookDir.transform.localRotation = aimManager.transform.rotation;
        lookDir.transform.position = playerHead.transform.position;
        lookDir.transform.rotation = aimManager.transform.rotation;
        lookDir.transform.Translate(0, 0, 5f);
        if (equippedWeapon)
        {
            inventory.equippedWeaponSlot.transform.LookAt(lookDir.transform);
            //inventory.equippedWeaponSlot.transform.Rotate(90, 0, 0);
            equippedWeapon.transform.LookAt(lookDir.transform);
            equippedWeapon.transform.Rotate(0, 90, 0);
        }
    }

    private void UpdateIK()
    {
        //Set ik goals to the weapon grip position and rotation
        if (equippedWeapon)
        {
            ik.rHandpos = equippedWeapon.transform.GetChild(1).position;
            ik.rHandrot = equippedWeapon.transform.GetChild(1).rotation;
            ik.lHandpos = equippedWeapon.transform.GetChild(2).position;
            ik.lHandrot = equippedWeapon.transform.GetChild(2).rotation;
            ik.rElbowpos = equippedWeapon.transform.GetChild(3).position;
            ik.lElbowpos = equippedWeapon.transform.GetChild(4).position;
        }
    }

    void CheckFire()
    {
        if (equippedWeapon != null)
        {
            if (Input.GetMouseButton(0))
            {
                Shoot();
            }
            else
                anim.SetBool("Shooting", false);
        }
        else if (Input.GetMouseButtonDown(0))
        {

            Punch();
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
            Gun weapon = equippedWeapon.GetComponent<Gun>();
            if (!weapon.GetReloading())
            {
                anim.SetBool("Interact", true);
                anim.SetTrigger("Reload");
                StartCoroutine(weapon.Reload(inventory.GetAmmo(), inventory));
            }
        }

    }

    void Punch()
    {
        if (Time.time >= nextTimeToFire)
        {

            anim.SetTrigger("Punch");
            nextTimeToFire = Time.time + 1f / 50f;
            StartCoroutine(PunchCalculation());
        }

    }

    IEnumerator PunchCalculation()
    {
        punchSoundPlayed = true;
        yield return new WaitForSeconds(0.5f);

        RaycastHit hit;
        if (Physics.Raycast(fist.transform.position, aimManager.transform.forward,
            out hit, 1.0f, mask))
        {

            if (hit.collider != null)
            {
                audioSource.clip = punchClip;
                audioSource.Play();
            }
            Debug.Log(hit.transform.name);

            //applies force to object away from player
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForce(-hit.normal * 50f);
            }

            //checks if target is an enemy and applies damage
            Status targetStatus = hit.transform.GetComponent<Status>();
            if (targetStatus)
            {
                targetStatus.DamageHealthAndArmor(10f);
            }
        }
        punchSoundPlayed = false;
    }

    void Shoot()
    {
        if (equippedWeapon.GetComponent<Gun>().GetCurrentAmmo() > 0 && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / equippedWeapon.GetComponent<Gun>().GetFireRate();
            anim.SetBool("Shooting", true);

            equippedWeapon.GetComponent<Gun>().PlayShotSound();
            GameObject muzzleFlash = Instantiate(equippedWeapon.GetComponent<Gun>().muzzleFlash, equippedWeapon.transform.GetChild(0).transform.position, equippedWeapon.transform.GetChild(0).transform.rotation);
            Destroy(muzzleFlash, 0.1f);

            RaycastHit hit;
            Vector3 aimManagerBack = aimManager.transform.forward.normalized;
            aimManagerBack.Scale(new Vector3(-1f, -1f, -1f));

            equippedWeapon.GetComponent<Gun>().DecreaseAmmo();

            Quaternion offsetAngle = Quaternion.AngleAxis(-2.2f, new Vector3(0, 1, 0));
            Vector3 shootVector = offsetAngle * aimManager.transform.forward;

            if (Physics.Raycast(equippedWeapon.transform.GetChild(0).transform.position, shootVector,
                    out hit, equippedWeapon.GetComponent<Gun>().GetRange(), mask))
            {
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
                Status targetStatus = hit.transform.GetComponent<Status>();
                if (targetStatus)
                {
                    targetStatus.DamageHealthAndArmor(equippedWeapon.GetComponent<Gun>().GetDamage());
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
            GameObject grenade = Instantiate(grenadePrefab, inventory.equippedWeaponSlot.transform.position + new Vector3(0, 0.2f, 0.11f), transform.rotation);
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            rb.AddForce((aimManager.transform.forward + new Vector3(0, 0.5f, 0)) * throwForce);
            StartCoroutine(grenade.GetComponent<Grenade>().Detonate());
            yield return new WaitForSeconds(1f);
            if (equippedWeapon == null && inventory.GetWeapons()[selectedWeapon] != null)
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
        }
        //weapon already equipped
        else
        {
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

    public void SetEquippedWeapon(GameObject weapon)
    {
        equippedWeapon = weapon;
        if (weapon == null)
            anim.SetBool("GunEquipped", false);
    }

}
*/