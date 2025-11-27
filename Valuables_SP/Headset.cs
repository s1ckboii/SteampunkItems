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

    private PhotonView _photonView;
    private PhysGrabObject _physGrabObject;
    private ItemToggle _toggle;
    private TextMeshPro _prompt;
    private Vector3 _direction;
    private Vector3 _scale;
    private VertexGradient _vertexGradient;

    private readonly float _speed = 0.2f;
    private float _gradientTime;
    private float _showTimer;
    private float _curveLerp;
    private float _currentTilt;
    private float _currentCamTilt;
    private float[] _audioSamples;
    private int _currentSongIndex;
    private bool _isPlaying;
    private bool _isFirstGrab = true;
    private string _promptInteract;
    private string _lastBlacklistString = null;

    private readonly List<int> _blacklistedSongs = [];
    private List<AudioClip> _ogSongs = [];
    private List<ParticleSystem> _ogParts = [];
    private List<PlayerAvatar> _allPlayers = [];

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bobRadiusOuter);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, bobRadiusInner);
    }

    private void Awake()
    {
        _prompt = GetComponentInChildren<TextMeshPro>();
        _vertexGradient = _prompt.colorGradient;
        _scale = _prompt.transform.localScale;
        _promptInteract = InputManager.instance.InputDisplayReplaceTags("[interact]");
        _toggle = GetComponent<ItemToggle>();
        _photonView = GetComponent<PhotonView>();
        _physGrabObject = GetComponent<PhysGrabObject>();
        _prompt.enabled = Plugins.ModConfig.ConfigPromptEnable.Value;

        _ogSongs = [.. songs];
        _ogParts = [.. particles];

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            ApplyBlacklist(Plugins.ModConfig.ConfigBlacklistedSongs.Value);
            _photonView.RPC("SyncBlacklistRPC", RpcTarget.Others, Plugins.ModConfig.ConfigBlacklistedSongs.Value);
        }
    }

    private void Start()
    {
        _allPlayers.AddRange(GameDirector.instance.PlayerList);
        _audioSamples = new float[sampleSize];
    }

    private void Update()
    {
        if (_physGrabObject.grabbed)
        {
            _showTimer = 0.1f;
            if (_physGrabObject.grabbedLocal)
            {
                audioSource.volume = Plugins.ModConfig.ConfigGrabbedMusicVolume.Value;
            }
            if (_isFirstGrab && Plugins.ModConfig.ConfigFirstGrab.Value)
            {
                _toggle.toggleState = true;
                _isFirstGrab = false;
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

        _prompt.transform.forward = _direction;
        _direction = PhysGrabber.instance.transform.forward;


        string currentBlacklist = Plugins.ModConfig.ConfigBlacklistedSongs.Value;

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            if (currentBlacklist != _lastBlacklistString)
            {
                _lastBlacklistString = currentBlacklist;
                UpdateBlacklist(currentBlacklist);
            }
        }

        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            _curveLerp += 10f * Time.deltaTime;
            _curveLerp = Mathf.Clamp01(_curveLerp);
            _prompt.transform.localScale = _scale * curveIntro.Evaluate(_curveLerp);
            return;
        }
        PromptGone();
    }
    private void ApplyHeadBobToNearbyPlayers()
    {
        audioSource.GetOutputData(_audioSamples, 0);
        float sum = 0f;
        for (int i = 0; i < _audioSamples.Length; i++)
            sum += Mathf.Abs(_audioSamples[i]);
        float rms = sum / _audioSamples.Length;

        float wave = Mathf.Sin(Time.time * 2f * Mathf.PI * 2f);

        foreach (PlayerAvatar player in _allPlayers)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(player.transform.position, transform.position);
            float tiltAmount = bobAmount;

            float camAmount;
            if (_physGrabObject.grabbedLocal)
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

            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * smoothSpeed);
            _currentCamTilt = Mathf.Lerp(_currentCamTilt, targetCamTilt, Time.deltaTime * smoothSpeed);

            if (player.isLocal)
            {
                CameraAim.Instance.AdditiveAimY(_currentCamTilt);
                player.playerAvatarVisuals.HeadTiltOverride(_currentTilt);
            }
            else
            {
                player.playerAvatarVisuals.HeadTiltOverride(_currentTilt);
            }
        }
    }

    private void PromptGone()
    {
        _curveLerp -= 10f * Time.deltaTime;
        _curveLerp = Mathf.Clamp01(_curveLerp);
        _prompt.transform.localScale = _scale * curveOutro.Evaluate(_curveLerp);
    }

    private void SetRandomGradientCorners()
    {
        if (_prompt == null)
        {
            return;
        }

        _vertexGradient.topLeft = topLeft.Evaluate((_gradientTime + 0f) % 1f);
        _vertexGradient.topRight = topRight.Evaluate((_gradientTime + 0.25f) % 1f);
        _vertexGradient.bottomLeft = bottomLeft.Evaluate((_gradientTime + 0.5f) % 1f);
        _vertexGradient.bottomRight = bottomRight.Evaluate((_gradientTime + 0.75f) % 1f);

        _prompt.colorGradient = _vertexGradient;
    }

    private void ToggleAudio()
    {
        if (_toggle.toggleState && songs.Count > 0)
        {
            _prompt.color = Color.white;
            _gradientTime += Time.deltaTime * _speed;
            if (_gradientTime > 1f)
            {
                _gradientTime -= 1f;
            }
            SetRandomGradientCorners();
            _prompt.enableVertexGradient = true;
            _prompt.text = "Toggle music OFF [" + _promptInteract + "]";

            int randomIndex = Random.Range(0, songs.Count);
            if (SemiFunc.IsMultiplayer())
            {
                _photonView.RPC("PlaySongRPC", RpcTarget.All, randomIndex);
            }
            else
            {
                PlaySongRPC(randomIndex);
            }
        }
        else if (songs.Count == 0)
        {
            _prompt.text = "NO MORE SONGS TO PLAY";
            _prompt.color = Color.red;
        }
        else
        {
            _prompt.color = Color.white;
            _prompt.enableVertexGradient = false;
            _prompt.text = "Toggle music ON [" + _promptInteract + "]";
            _isPlaying = false;
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
        songs = [.. _ogSongs];
        particles = [.. _ogParts];

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
                if (index >= 0 && index < _ogSongs.Count)
                {
                    blacklist.Add(index);
                }
                else
                {
                    Debug.LogWarning($"Blacklist index {index} out of range. Valid range is 0 to {_ogSongs.Count - 1}.");
                }
            }
            else
            {
                Debug.LogWarning($"Blacklist entry '{trimmed}' is not valid, please use integers from 0 to {_ogSongs.Count - 1} (you baakaa)");
            }
        }

        List<AudioClip> newSongs = [];
        List<ParticleSystem> newParticles = [];

        for (int i = 0; i < _ogSongs.Count; i++)
        {
            if (!blacklist.Contains(i))
            {
                newSongs.Add(_ogSongs[i]);
                if (i < _ogParts.Count)
                {
                    newParticles.Add(_ogParts[i]);
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
        _photonView.RPC("SyncBlacklistRPC", RpcTarget.Others, blacklistString);
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

        if (!_isPlaying)
        {
            _isPlaying = true;

            audioSource.clip = songs[songIndex];
            audioSource.Play();

            _currentSongIndex = songIndex;

            PlayParticles(songIndex);
        }
    }
}