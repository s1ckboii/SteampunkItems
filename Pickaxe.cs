﻿using Photon.Pun;
using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    public Animator animator;
    private PhysGrabObject physGrabObject;

    private PhotonView photonView;

    private bool isGrabbed = false;
    private bool wasGrabbedOnPreviousUpdate;

    private void Awake()
    {
        physGrabObject = GetComponent<PhysGrabObject>();
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (physGrabObject != null)
        {
            isGrabbed = physGrabObject.grabbed;
        }

        if (isGrabbed != wasGrabbedOnPreviousUpdate)
        {
            photonView.RPC("SetAnimationState", RpcTarget.All, isGrabbed);
        }

        wasGrabbedOnPreviousUpdate = isGrabbed;
    }

    [PunRPC]
    public void SetAnimationState(bool grab)
    {
        if (grab)
        {
            animator.SetBool("Grab", true);
            animator.SetBool("Release", false);
        }
        else
        {
            animator.SetBool("Release", true);
            animator.SetBool("Grab", false);
        }
    }
}
