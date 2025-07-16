/*
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

namespace SteampunkItems.Valuables_SP;

public class MagniGlass : MonoBehaviour
{
    private PhotonView photonView;
    private PhysGrabObject physGrabObject;
    private ItemToggle toggle;

    public AnimationCurve curveIntro;
    public AnimationCurve curveOutro;
    private TextMeshPro prompt;
    private Transform forceGrabPoint;
    private Vector3 scale;
    private Vector3 dir;

    private string promptInteract;
    private bool stateStart;
    private bool wasGrabbedLastFrame = false;
    private int ownerActorNumber = -1;
    private float showTimer;
    private float curveLerp;

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
        prompt = GetComponent<TextMeshPro>();
        promptInteract = InputManager.instance.InputDisplayReplaceTags("[interact]");
        toggle = GetComponent<ItemToggle>();

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
            showTimer = 0.1f;
            if (physGrabObject.grabbedLocal)
            {
                prompt.text = "Zoom in [" + promptInteract + "]";
                if (showTimer > 0f)
                {
                    showTimer -= Time.deltaTime;
                    curveLerp += 10f * Time.deltaTime;
                    curveLerp = Mathf.Clamp01(curveLerp);
                    prompt.transform.localScale = scale * curveIntro.Evaluate(curveLerp);
                    return;
                }
                if (toggle.toggleState)
                {
                    SetState(States.Active);
                    curveLerp -= 10f * Time.deltaTime;
                    curveLerp = Mathf.Clamp01(curveLerp);
                    prompt.transform.localScale = scale * curveOutro.Evaluate(curveLerp);
                }
                prompt.transform.forward = dir;
                dir = PhysGrabber.instance.transform.forward;
            }
            curveLerp -= 10f * Time.deltaTime;
            curveLerp = Mathf.Clamp01(curveLerp);
            prompt.transform.localScale = scale * curveOutro.Evaluate(curveLerp);
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
            PhysGrabber.instance.OverrideGrabDistance(Plugins.ModConfig.ConfigOverrideGrabDistance.Value);
            PlayerAvatar.instance.OverridePupilSize(Plugins.ModConfig.ConfigOverrideMagniGlassPupilSize.Value, 4, 1f, 1f, 5f, 0.5f);
            CameraZoom.Instance.OverrideZoomSet(Plugins.ModConfig.ConfigOverrideMagniGlassZoomSet.Value, 0.1f, 0.5f, 1f, gameObject, 0);
        }

        if (!isGrabbing && wasGrabbedLastFrame && ownerActorNumber == localActor)
        {
            ownerActorNumber = -1;
        }

        if (toggle.toggleState || !isGrabbing)
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
}*/