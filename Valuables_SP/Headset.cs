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

    [Header("Head Bob Settings")]
    public float bobRadiusInner = 5f;
    public float bobRadiusOuter = 10f;
    public float bobAmount = 15f;
    public float camAmountMax = 0.02f;
    public float camAmountGrab = 0.01f;
    public int sampleSize = 256;
    public float smoothSpeed = 5f;

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
    private float currentTilt;
    private float currentCamTilt;
    private float[] audioSamples;
    private int _currentSongIndex;
    private bool isPlaying;
    private bool isFirstGrab = true;
    private string promptInteract;
    private string lastBlacklistString = null;

    private readonly List<int> blacklistedSongs = [];
    private List<AudioClip> ogSongs = [];
    private List<ParticleSystem> ogParts = [];
    private List<PlayerAvatar> allPlayers = [];

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bobRadiusOuter);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, bobRadiusInner);
    }

    private void Awake()
    {
        prompt = GetComponentInChildren<TextMeshPro>();
        vg = prompt.colorGradient;
        scale = prompt.transform.localScale;
        promptInteract = InputManager.instance.InputDisplayReplaceTags("[interact]");
        toggle = GetComponent<ItemToggle>();
        photonView = GetComponent<PhotonView>();
        physGrabObject = GetComponent<PhysGrabObject>();
        prompt.enabled = Plugins.ModConfig.ConfigPromptEnable.Value;

        ogSongs = [.. songs];
        ogParts = [.. particles];

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            ApplyBlacklist(Plugins.ModConfig.ConfigBlacklistedSongs.Value);
            photonView.RPC("SyncBlacklistRPC", RpcTarget.Others, Plugins.ModConfig.ConfigBlacklistedSongs.Value);
        }
    }

    private void Start()
    {
        allPlayers.AddRange(GameDirector.instance.PlayerList);
        audioSamples = new float[sampleSize];
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

        if (audioSource.isPlaying && audioSource.clip != null)
        {
            ApplyHeadBobToNearbyPlayers();
        }

        prompt.transform.forward = dir;
        dir = PhysGrabber.instance.transform.forward;


        string currentBlacklist = Plugins.ModConfig.ConfigBlacklistedSongs.Value;

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            if (currentBlacklist != lastBlacklistString)
            {
                lastBlacklistString = currentBlacklist;
                UpdateBlacklist(currentBlacklist);
            }
        }

        if (showTimer > 0f)
        {
            showTimer -= Time.deltaTime;
            curveLerp += 10f * Time.deltaTime;
            curveLerp = Mathf.Clamp01(curveLerp);
            prompt.transform.localScale = scale * curveIntro.Evaluate(curveLerp);
            return;
        }
        PromptGone();
    }
    private void ApplyHeadBobToNearbyPlayers()
    {
        audioSource.GetOutputData(audioSamples, 0);
        float sum = 0f;
        for (int i = 0; i < audioSamples.Length; i++)
            sum += Mathf.Abs(audioSamples[i]);
        float rms = sum / audioSamples.Length;

        float wave = Mathf.Sin(Time.time * 2f * Mathf.PI * 2f);

        foreach (PlayerAvatar player in allPlayers)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(player.transform.position, transform.position);
            float tiltAmount = bobAmount;

            float camAmount;
            if (physGrabObject.grabbedLocal)
            {
                camAmount = camAmountGrab;
            }
            else if (distance <= bobRadiusOuter)
            {
                float t = Mathf.InverseLerp(bobRadiusOuter, bobRadiusInner, distance);
                camAmount = Mathf.Lerp(0f, camAmountMax, t);
                tiltAmount = bobAmount * t;
            }
            else
            {
                continue;
            }

            float targetTilt = wave * rms * tiltAmount * 100f;
            float targetCamTilt = wave * rms * camAmount * 100f;

            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * smoothSpeed);
            currentCamTilt = Mathf.Lerp(currentCamTilt, targetCamTilt, Time.deltaTime * smoothSpeed);

            if (player.isLocal)
            {
                CameraAim.Instance.AdditiveAimY(currentCamTilt);
                player.playerAvatarVisuals.HeadTiltOverride(currentTilt);
            }
            else
            {
                player.playerAvatarVisuals.HeadTiltOverride(currentTilt);
            }
        }
    }

    private void PromptGone()
    {
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

    private void ToggleAudio()
    {
        if (toggle.toggleState && songs.Count > 0)
        {
            prompt.color = Color.white;
            gradientTime += Time.deltaTime * speed;
            if (gradientTime > 1f)
            {
                gradientTime -= 1f;
            }
            SetRandomGradientCorners();
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
        else if (songs.Count == 0)
        {
            prompt.text = "NO MORE SONGS TO PLAY";
            prompt.color = Color.red;
        }
        else
        {
            prompt.color = Color.white;
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

    private void ApplyBlacklist(string blacklistString)
    {
        songs = [.. ogSongs];
        particles = [.. ogParts];

        if (string.IsNullOrWhiteSpace(blacklistString))
        {
            return;
        }

        string[] split = blacklistString.Split(',');

        HashSet<int> blacklist = [];

        foreach (string s in split)
        {
            string trimmed = s.Trim();

            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            if (int.TryParse(trimmed, out int index))
            {
                if (index >= 0 && index < ogSongs.Count)
                {
                    blacklist.Add(index);
                }
                else
                {
                    Debug.LogWarning($"Blacklist index {index} out of range. Valid range is 0 to {ogSongs.Count - 1}.");
                }
            }
            else
            {
                Debug.LogWarning($"Blacklist entry '{trimmed}' is not valid, please use integers from 0 to {ogSongs.Count - 1} (you baakaa)");
            }
        }

        List<AudioClip> newSongs = [];
        List<ParticleSystem> newParticles = [];

        for (int i = 0; i < ogSongs.Count; i++)
        {
            if (!blacklist.Contains(i))
            {
                newSongs.Add(ogSongs[i]);
                if (i < ogParts.Count)
                {
                    newParticles.Add(ogParts[i]);
                }
            }
        }

        songs = newSongs;
        particles = newParticles;

        if (songs.Count == 0)
        {
            Debug.LogWarning("All song-particle pairs have been blacklisted.");
        }
    }
    public void UpdateBlacklist(string blacklistString)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        ApplyBlacklist(blacklistString);
        photonView.RPC("SyncBlacklistRPC", RpcTarget.Others, blacklistString);
    }


    [PunRPC]
    private void SyncBlacklistRPC(string blacklistString)
    {
        ApplyBlacklist(blacklistString);
    }

    [PunRPC]
    private void PlaySongRPC(int songIndex)
    {
        if (songs == null || songs.Count == 0)
        {
            Debug.LogWarning("Every song is blacklisted...");
            return;
        }

        if (songIndex < 0 || songIndex >= songs.Count)
        {
            Debug.LogWarning($"Invalid song index {songIndex}. Valid range is 0 to {songs.Count - 1}.");
            return;
        }

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