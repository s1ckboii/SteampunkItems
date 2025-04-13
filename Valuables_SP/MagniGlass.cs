using Photon.Pun;
using UnityEngine;

namespace SteampunkItems.Valuables_SP;
public class MagniGlass : MonoBehaviour
{
    private PhotonView _photonView;
    private PhysGrabObject _physGrabObject;
    private bool _stateStart;

    public States _currentState;
    private bool _effectsApplied = false;
    private bool _wasGrabbedLastFrame = false;
    private int _previousOwnerActorNumber = -1;
    private Transform _forceGrabPoint;

    public enum States
    {
        Idle,
        Active
    }
    public void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _physGrabObject = GetComponent<PhysGrabObject>();
        _forceGrabPoint = transform.Find("Force Grab Point");
        _physGrabObject.forceGrabPoint = _forceGrabPoint;
    }
    public void Update()
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
    public void StateActive()
    {
        if (_stateStart)
        {
            if (SemiFunc.IsMultiplayer())
            {
                bool isGrabbing = _physGrabObject.grabbedLocal;
                bool isOwner = _photonView.IsMine;

                if (isGrabbing && !isOwner && HasOwnerDropped())
                {
                    _photonView.RequestOwnership();
                }

                if (isOwner && isGrabbing && !_effectsApplied)
                {
                    _photonView.RPC("ApplyEffects", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
                }

                if (isOwner && !isGrabbing && _wasGrabbedLastFrame)
                {
                    _photonView.RPC("ResetEffects", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
                }

                _wasGrabbedLastFrame = isGrabbing;
            }
            else
            {
                if (_physGrabObject.grabbed)
                {
                    ApplyEffects(0);
                }
                else
                {
                    ResetEffects(0);
                }
            }
        }
    }
    public void StateIdle()
    {
        if (_stateStart)
        {
            _stateStart = false;
        }
        if (_physGrabObject.grabbedLocal)
        {
            SetState(States.Active);
        }
    }
    public bool HasOwnerDropped()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == _photonView.OwnerActorNr)
            return !_physGrabObject.grabbedLocal;

        return _previousOwnerActorNumber == -1;
    }
    [PunRPC]
    public void SetState(States state)
    {
        _currentState = state;
        _stateStart = true;
    }
    [PunRPC]
    public void ApplyEffects(int targetActor)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != targetActor)
        {
            return;
        }
        _effectsApplied = true;
        _previousOwnerActorNumber = _photonView.OwnerActorNr;

        PlayerAvatar.instance.OverridePupilSize(3f, 4, 1f, 1f, 5f, 0.5f);
        CameraZoom.Instance.OverrideZoomSet(40f, 0.1f, 0.5f, 1f, gameObject, 0);

        Debug.Log("Applying magniglass effects to local player");

    }
    [PunRPC]
    public void ResetEffects(int targetActor)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != targetActor)
        {
            return;
        }
        _effectsApplied = false;
        _previousOwnerActorNumber = -1;

        StateIdle();

        Debug.Log("Resetting magniglass effects on local player");
    }
}