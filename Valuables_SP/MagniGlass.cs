using Photon.Pun;
using System.Collections;
using UnityEngine;


namespace SteampunkItems.Valuables_SP;

public class MagniGlass : MonoBehaviour
{
    private PhotonView photonView;
    private PhysGrabObject physGrabObject;
    private Transform forceGrabPoint;

    private bool stateStart;
    private bool wasGrabbedLastFrame = false;
    private int ownerActorNumber = -1;

    private Coroutine resetIndestructibleCoroutine;

    public States _currentState;
    public enum States
    {
        Idle,
        Active
    }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        physGrabObject = GetComponent<PhysGrabObject>();

        forceGrabPoint = transform.Find("Force Grab Point");
    }

    private void Update()
    {
        switch (_currentState)
        {
            case States.Idle:
                StateIdle();
                break;
            case States.Active:
                StateActive();
                break;
        }
    }
    private void StateIdle()
    {
        if (stateStart)
        {
            stateStart = false;
        }

        if (physGrabObject.grabbedLocal)
        {
            SetState(States.Active);
        }
    }
    private void StateActive()
    {
        if (stateStart)
        {
            stateStart = false;
        }
        bool isGrabbing = physGrabObject.grabbedLocal;
        int localActor = PhotonNetwork.LocalPlayer.ActorNumber;

        if (isGrabbing)
        {
            if (ownerActorNumber != localActor)
            {
                ownerActorNumber = localActor;
            }

            if (resetIndestructibleCoroutine != null)
            {
                StopCoroutine(resetIndestructibleCoroutine);
            }

            physGrabObject.OverrideIndestructible(0.5f);
            resetIndestructibleCoroutine = StartCoroutine(ResetIndestructibleAfterDelay(0.5f));
        }

        if (isGrabbing && ownerActorNumber == localActor)
        {
            physGrabObject.forceGrabPoint = forceGrabPoint;
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("ForcePosition", RpcTarget.All);
            }
            else
            {
                ForcePosition();
            }
            PhysGrabber.instance.OverrideGrabDistance(0.5f);
            PlayerAvatar.instance.OverridePupilSize(3f, 4, 1f, 1f, 5f, 0.5f);
            CameraZoom.Instance.OverrideZoomSet(40f, 0.1f, 0.5f, 1f, gameObject, 0);
        }

        if (!isGrabbing && wasGrabbedLastFrame && ownerActorNumber == localActor)
        {
            ownerActorNumber = -1;
        }

        if (!isGrabbing)
        {
            SetState(States.Idle);
        }

        wasGrabbedLastFrame = isGrabbing;
    }
    private void SetState(States newState)
    {
        _currentState = newState;
        stateStart = true;
    }
    [PunRPC]
    public void ForcePosition()
    {
        Quaternion turnX = Quaternion.Euler(-30f, 0f, 10f);
        Quaternion turnY = Quaternion.Euler(50f, 180f, 0f);
        Quaternion identity = Quaternion.identity;

        
        bool flag = false;
        if (!flag)
        {
            physGrabObject.TurnXYZ(turnX, turnY, identity);
        }
        physGrabObject.OverrideGrabVerticalPosition(-0.24f);
    }
    private IEnumerator ResetIndestructibleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        physGrabObject.OverrideIndestructible(0f);
        resetIndestructibleCoroutine = null;
    }

}