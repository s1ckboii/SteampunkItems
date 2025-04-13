using Photon.Pun;
using System.Collections;
using UnityEngine;


namespace SteampunkItems.Valuables_SP;

public class MagniGlass : MonoBehaviour
{
    private PhotonView _photonView;
    private PhysGrabObject _physGrabObject;
    private Transform _forceGrabPoint;

    private bool _stateStart;
    private bool _wasGrabbedLastFrame = false;
    private int _ownerActorNumber = -1;

    private Coroutine _resetIndestructibleCoroutine;

    public States _currentState;
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
    private void StateIdle()
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
    private void StateActive()
    {
        if (_stateStart)
        {
            _stateStart = false;
        }
        bool isGrabbing = _physGrabObject.grabbedLocal;
        int localActor = PhotonNetwork.LocalPlayer.ActorNumber;

        if (isGrabbing)
        {
            if (_ownerActorNumber != localActor)
            {
                _ownerActorNumber = localActor;
            }

            if (_resetIndestructibleCoroutine != null)
            {
                StopCoroutine(_resetIndestructibleCoroutine);
            }

            _physGrabObject.OverrideIndestructible(0.5f);
            _resetIndestructibleCoroutine = StartCoroutine(ResetIndestructibleAfterDelay(0.5f));
        }

        if (isGrabbing && _ownerActorNumber == localActor)
        {
            _physGrabObject.forceGrabPoint = _forceGrabPoint;
            ForcePosition();
            PlayerAvatar.instance.OverridePupilSize(3f, 4, 1f, 1f, 5f, 0.5f);
            CameraZoom.Instance.OverrideZoomSet(20f, 0.1f, 0.5f, 1f, gameObject, 0);
        }

        if (!isGrabbing && _wasGrabbedLastFrame && _ownerActorNumber == localActor)
        {
            _ownerActorNumber = -1;
        }

        if (!isGrabbing)
        {
            SetState(States.Idle);
        }

        _wasGrabbedLastFrame = isGrabbing;
    }
    public void SetState(States newState)
    {
        _currentState = newState;
        _stateStart = true;
    }
    public void ForcePosition()
    {
        Quaternion turnX = Quaternion.Euler(-30f, 0f, 10f);
        Quaternion turnY = Quaternion.Euler(50f, 180f, 0f);
        Quaternion identity = Quaternion.identity;

        
        bool flag = false;
        if (!flag)
        {
            _physGrabObject.TurnXYZ(turnX, turnY, identity);
        }
        _physGrabObject.OverrideGrabVerticalPosition(-0.24f);
        PhysGrabber.instance.OverrideGrabDistance(0.5f);
    }
    private IEnumerator ResetIndestructibleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _physGrabObject.OverrideIndestructible(0f);
        _resetIndestructibleCoroutine = null;
    }

}