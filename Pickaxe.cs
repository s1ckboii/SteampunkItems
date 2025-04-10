using UnityEngine;
using Photon.Pun;

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
        if (SemiFunc.IsMultiplayer())
        {
            if (isGrabbed != wasGrabbedOnPreviousUpdate)
            {
                photonView.RPC("SetAnimationState", RpcTarget.All, isGrabbed);
            }
        }
        else
        {
            if (isGrabbed != wasGrabbedOnPreviousUpdate)
            {
                SetAnimationState(isGrabbed);
            }
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
