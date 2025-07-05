using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SteampunkItems.Valuables_SP;
public class Headset : MonoBehaviour
{
    public AudioSource audioSource;
    public List<ParticleSystem> particles = [];
    public List<AudioClip> songs = [];
    public AnimationCurve curveIntro;
    public AnimationCurve curveOutro;
    public Gradient topLeft;
    public Gradient topRight;
    public Gradient bottomLeft;
    public Gradient bottomRight;

    private PhotonView photonView;
    private PhysGrabObject physGrabObject;
    private ItemToggle toggle;
    private TextMeshPro prompt;
    private Vector3 dir;
    private Vector3 scale;
    private VertexGradient vg;

    private readonly float speed = 0.2f;
    private float gradientTime;
    private float showTimer;
    private float curveLerp;
    private int _currentSongIndex;
    private bool isPlaying;
    private bool isFirstGrab = true;
    private string promptInteract;

    private void Awake()
    {
        prompt = GetComponentInChildren<TextMeshPro>();
        vg = prompt.colorGradient;
        scale = prompt.transform.localScale;
        promptInteract = InputManager.instance.InputDisplayReplaceTags("[interact]");
        toggle = GetComponent<ItemToggle>();
        photonView = GetComponent<PhotonView>();
        physGrabObject = GetComponent<PhysGrabObject>();
    }
    private void Update()
    {
        if (physGrabObject.grabbed)
        {
            showTimer = 0.1f;
            if (physGrabObject.grabbedLocal)
            {
                audioSource.volume = Plugins.ModConfig.ConfigGrabbedMusicVolume.Value;
            }
            if (isFirstGrab && Plugins.ModConfig.ConfigFirstGrab.Value)
            {
                toggle.toggleState = true;
                isFirstGrab = false;
            }
            ToggleAudio();
        }
        else
        {
            audioSource.volume -= Time.deltaTime * 0.25f;
            audioSource.volume = Mathf.Max(audioSource.volume, Plugins.ModConfig.ConfigUngrabbedMusicVolume.Value);
        }

        prompt.transform.forward = dir;
        dir = PhysGrabber.instance.transform.forward;
        

        if (showTimer > 0f)
        {
            showTimer -= Time.deltaTime;
            curveLerp += 10f * Time.deltaTime;
            curveLerp = Mathf.Clamp01(curveLerp);
            prompt.transform.localScale = scale * curveIntro.Evaluate(curveLerp);
            return;
        }
        curveLerp -= 10f * Time.deltaTime;
        curveLerp = Mathf.Clamp01(curveLerp);
        prompt.transform.localScale = scale * curveOutro.Evaluate(curveLerp);
    }

    private void SetRandomGradientCorners()
    {
        if (prompt == null)
        {
            return;
        }

        vg.topLeft = topLeft.Evaluate((gradientTime + 0f) % 1f);
        vg.topRight = topRight.Evaluate((gradientTime + 0.25f) % 1f);
        vg.bottomLeft = bottomLeft.Evaluate((gradientTime + 0.5f) % 1f);
        vg.bottomRight = bottomRight.Evaluate((gradientTime + 0.75f) % 1f);

        prompt.colorGradient = vg;
    }

    private void AnimateGlowHeartbeat()
    {
        if (prompt == null) return;

        Material mat = prompt.fontMaterial;

        mat.EnableKeyword("GLOW_ON");

        float minGlow = 0.02f;
        float maxGlow = 0.2f;

        float pulse = minGlow + (maxGlow - minGlow) * (0.5f + 0.5f * Mathf.Sin(Time.time * 15f));

        mat.SetFloat("_GlowPower", pulse);

        Color baseGlowColor = Color.green;
        Color glowColor = baseGlowColor * pulse;
        glowColor.a = 1f;
        mat.SetColor("_GlowColor", glowColor);
    }

    private void ToggleAudio()
    {
        if (toggle.toggleState)
        {
            gradientTime += Time.deltaTime * speed;
            if (gradientTime > 1f)
            {
                gradientTime -= 1f;
            }
            SetRandomGradientCorners();
            AnimateGlowHeartbeat();
            prompt.enableVertexGradient = true;
            prompt.text = "Toggle music OFF [" + promptInteract + "]";
            int randomIndex = Random.Range(0, songs.Count);
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("PlaySongRPC", RpcTarget.All, randomIndex);
            }
            else
            {
                PlaySongRPC(randomIndex);
            }
        }
        else if (!toggle.toggleState)
        {
            prompt.enableVertexGradient = false;
            prompt.text = "Toggle music ON [" + promptInteract + "]";
            isPlaying = false;
            StopParticles();
            audioSource.Stop();
        }
    }
    private void PlayParticles(int songIndex)
    {
        StopParticles();

        if (songIndex < particles.Count)
        {
            particles[songIndex].Play();
        }
    }
    private void StopParticles()
    {
        foreach (var particle in particles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    [PunRPC]
    private void PlaySongRPC(int songIndex)
    {
        if (!isPlaying)
        {
            isPlaying = true;

            audioSource.clip = songs[songIndex];
            audioSource.Play();

            _currentSongIndex = songIndex;

            PlayParticles(songIndex);
        }
    }
}
