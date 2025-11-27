using UnityEngine;
using Photon.Pun;

namespace SteampunkItems.Items_SP;
public class Pickaxe : MonoBehaviour
{
    public Animator animator;

    private PhysGrabObject _physGrabObject;
    private PhotonView _photonView;

    private bool _isGrabbed = false;
    private bool _wasGrabbedOnPreviousUpdate;

    private void Awake()
    {
        _physGrabObject = GetComponent<PhysGrabObject>();
        _photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (_physGrabObject != null)
        {
            _isGrabbed = _physGrabObject.grabbed;
        }
        if (SemiFunc.IsMultiplayer())
        {
            if (_isGrabbed != _wasGrabbedOnPreviousUpdate)
            {
                _photonView.RPC("SetAnimationState", RpcTarget.All, _isGrabbed);
            }
        }
        else
        {
            if (_isGrabbed != _wasGrabbedOnPreviousUpdate)
            {
                SetAnimationState(_isGrabbed);
            }
        }

            _wasGrabbedOnPreviousUpdate = _isGrabbed;
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
