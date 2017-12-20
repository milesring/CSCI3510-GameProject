using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Player;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour {
    protected Animator animator;
    AnimatorStateInfo animState;
    
    
    public Vector3 rHandpos, lHandpos, rElbowpos, lElbowpos;
    public Quaternion rHandrot, lHandrot;

    public bool ikActive;
    WeaponManager weaponManager;
    Inventory inventory;
    // Use this for initialization
	void Start () {
        inventory = GetInventory();
        animator = GetComponent<Animator>();
        animState = animator.GetCurrentAnimatorStateInfo(4);
        weaponManager = GetComponent<WeaponManager>() == null ? GetComponent<AIWeaponManager>() : GetComponent<WeaponManager>();
        rHandpos = new Vector3();
        lHandpos = new Vector3();
        rElbowpos = new Vector3();
        lElbowpos = new Vector3();
        rHandrot = new Quaternion();
        lHandrot = new Quaternion();
    }

    Inventory GetInventory() {
        if (GetComponent<PlayerInventory>() == null)
            return GetComponent<AIInventoryManager>();
        else
            return GetComponent<PlayerInventory>();
    }
	
    //callback function for calculating IK
	void OnAnimatorIK(int layerIndex)
    {
        if(animator)
        {
            if (ikActive)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, rHandpos);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rHandrot);

                animator.SetIKPosition(AvatarIKGoal.LeftHand, lHandpos);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, lHandrot);

                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rElbowpos);

                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, lElbowpos);
                //Don't set weights during certain animations
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.5f);
                if (weaponManager.GetEquippedWeapon() == null || !weaponManager.GetEquippedWeapon().GetComponent<Gun>().isReloading 
                    && !inventory.pickup && !animator.GetBool("UseMeds"))
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0.5f);
                }
                if(!animState.IsName("Start"))
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            }
        }
    }
}
