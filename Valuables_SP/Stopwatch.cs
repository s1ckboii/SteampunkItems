using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace SteampunkItems.Valuables_SP;

public class Stopwatch : MonoBehaviour
{
    public Transform particleSystemTransform;
    public ParticleSystem particleSystemSwirl;
    public ParticleSystem particleSystemGlitter;

    public MeshRenderer meshRenderer;

    public Transform secNeedle;
    public Transform minNeedle;
    public Transform hourNeedle;
    private Quaternion secTargetRot;
    private Quaternion minTargetRot;
    private Quaternion hourTargetRot;
    private float secNeedleSpeed;
    private float minNeedleSpeed;
    private float hourNeedleSpeed;
    private Vector2 minNeedleSpeedRange = new(30f, 90f);
    private Vector2 hourNeedleSpeedRange = new(10f, 40f);
    private Vector2 secNeedleSpeedRange = new(60f, 180f);

    #region ConfigNumbers
    private float PlayerSpeed;
    private float LookSpeed;
    private float ZoomSet;
    private float Saturation;

    #endregion

    public Light light;

    public Sound soundLoop;

    private PhysGrabObject physGrabObject;
    private PhotonView photonView;
    private Color color;

    private States currentState;

    private bool stateStart;
    private float soundPitchLerp;
    private int particleFocus;


    private enum States
    {
        Idle,
        Active
    }

    private void Awake()
    {
        physGrabObject = GetComponent<PhysGrabObject>();
        photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        float pitchMultiplier = Mathf.Lerp(2f, 0.5f, soundPitchLerp);
        soundLoop.PlayLoop(currentState == States.Active, 0.8f, 0.8f, pitchMultiplier);
        switch (currentState)
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
            particleSystemGlitter.Stop();
            particleSystemSwirl.Stop();
            stateStart = false;
        }
        if (physGrabObject.grabbed)
        {
            SetState(States.Active);
        }
        light.intensity = Mathf.Lerp(light.intensity, 0f, Time.deltaTime * 10f);
        soundPitchLerp = Mathf.Lerp(soundPitchLerp, 0f, Time.deltaTime * 10f);
        ColorUtility.TryParseHtmlString(Plugins.ModConfig.ConfigStopwatchOFFMaterial.Value, out color);
        meshRenderer.material.SetColor("_EmissionColor", color);
        if (light.intensity < 0.01f)
        {
            light.gameObject.SetActive(false);
        }
    }

    private void StateActive()
    {
        if (stateStart)
        {
            particleSystemGlitter.Play();
            particleSystemSwirl.Play();
            stateStart = false;
            light.gameObject.SetActive(true);
            GenerateNewNeedleTargets();
        }
        if (!light.gameObject.activeSelf)
        {
            light.gameObject.SetActive(true);
        }
        if (particleSystemTransform.gameObject.activeSelf)
        {
            HasValidTarget();
        }
        soundPitchLerp = Mathf.Lerp(soundPitchLerp, 1f, Time.deltaTime * 2f);
        light.intensity = Mathf.Lerp(light.intensity, 4f, Time.deltaTime * 2f);
        ColorUtility.TryParseHtmlString(Plugins.ModConfig.ConfigStopwatchONMaterial.Value, out color);
        meshRenderer.material.SetColor("_EmissionColor", color * light.intensity);
        foreach (PhysGrabber item in physGrabObject.playerGrabbing)
        {
            if ((bool)item && !item.isLocal)
            {
                item.playerAvatar.voiceChat.OverridePitch(Plugins.ModConfig.ConfigOthersVoicePitchMultiplier.Value, 1f, 2f);
            }
        }
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            physGrabObject.OverrideDrag(Plugins.ModConfig.ConfigOverrideDrag.Value);
            physGrabObject.OverrideAngularDrag(Plugins.ModConfig.ConfigOverrideAngularDrag.Value);
            PlayerSpeed = Plugins.ModConfig.ConfigOverridePlayerSpeed.Value;
            LookSpeed = Plugins.ModConfig.ConfigOverridePlayerLookSpeed.Value;
            ZoomSet = Plugins.ModConfig.ConfigOverrideStopwatchZoomSet.Value;
            Saturation = Plugins.ModConfig.ConfigSaturationOverride.Value;
            photonView.RPC("SyncConfigValuesRPC", RpcTarget.Others, PlayerSpeed, ZoomSet, LookSpeed, Saturation);
            if (!physGrabObject.grabbed)
            {
                SetState(States.Idle);
            }
        }
        if (physGrabObject.grabbedLocal)
        {
            PlayerAvatar.instance.voiceChat.OverridePitch(Plugins.ModConfig.ConfigOwnVoicePitchMultiplier.Value, 1f, 2f);
            PlayerAvatar.instance.OverridePupilSize(Plugins.ModConfig.ConfigOverrideStopwatchPupilSize.Value, 4, 1f, 1f, 5f, 0.5f);
            PlayerController.instance.OverrideSpeed(PlayerSpeed);
            PlayerController.instance.OverrideLookSpeed(PlayerSpeed + LookSpeed, 2f, 1f);
            PlayerController.instance.OverrideAnimationSpeed(0.2f, 1f, 2f);
            PlayerController.instance.OverrideTimeScale(0.1f);
            physGrabObject.OverrideTorqueStrength(0.6f);
            CameraZoom.Instance.OverrideZoomSet(ZoomSet, 0.1f, 0.5f, 1f, base.gameObject, 0);
            PostProcessing.Instance.SaturationOverride(Saturation, 0.1f, 0.5f, 0.1f, base.gameObject);
            RotateNeedleTowards(minNeedle, ref minTargetRot, minNeedleSpeed);
            RotateNeedleTowards(hourNeedle, ref hourTargetRot, hourNeedleSpeed);
            RotateNeedleTowards(secNeedle, ref secTargetRot, secNeedleSpeed);
        }


    }

    private void HasValidTarget()
    {
        List<PhysGrabber> grabbers = physGrabObject.playerGrabbing;
        if (grabbers.Count > particleFocus)
        {
            PhysGrabber grabber = grabbers[particleFocus];
            if ((bool)grabber)
            {
                Transform headLookAtTransform = grabber.playerAvatar.playerAvatarVisuals.headLookAtTransform;
                if ((bool) headLookAtTransform)
                {
                    particleSystemTransform.LookAt(headLookAtTransform);
                }
                particleFocus++;
            }
            else { particleFocus = 0; }
        }
        else { particleFocus = 0; }
    }

    private void GenerateNewNeedleTargets()
    {
        minTargetRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        hourTargetRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        secTargetRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        minNeedleSpeed = Random.Range(minNeedleSpeedRange.x, minNeedleSpeedRange.y);
        hourNeedleSpeed = Random.Range(hourNeedleSpeedRange.x, hourNeedleSpeedRange.y);
        secNeedleSpeed = Random.Range(secNeedleSpeedRange.x, secNeedleSpeedRange.y);
    }
    private void RotateNeedleTowards(Transform needle, ref Quaternion target, float speed)
    {
        needle.localRotation = Quaternion.RotateTowards(needle.localRotation, target, speed * Time.deltaTime);
        if (Quaternion.Angle(needle.localRotation, target) < 1f)
        {
            target = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            speed = Random.Range(30f, 90f);
        }
    }
    private void SetState(States state)
    {
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            if (!SemiFunc.IsMultiplayer())
            {
                SetStateRPC(state);
                return;
            }

        }
        photonView.RPC("SetStateRPC", RpcTarget.All, state);
    }

    [PunRPC]
    private void SetStateRPC(States state)
    {
        currentState = state;
        stateStart = true;
    }
    [PunRPC]
    private void SyncConfigValuesRPC(float playerSpeed, float zoomSet, float lookSpeed, float saturation)
    {
        PlayerSpeed = playerSpeed;
        ZoomSet = zoomSet;
        LookSpeed = lookSpeed;
        Saturation = saturation;
    }
}
