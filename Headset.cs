using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public class Headset : MonoBehaviour
{
    public AudioSource audioSource;

    public List<ParticleSystem> _particles;
    public List<AudioClip> _songs = [];

    private PhotonView _photonView;
    private PhysGrabObject _physGrabObject;

    private int _currentSongIndex;
    private ItemToggle _toggle;
    private bool _isPlaying;
    private bool _isFirstGrab = true;

    private readonly Dictionary<AudioClip, ParticleSystem> _songParticleMap = [];

    private void Awake()
    {
        _toggle = GetComponent<ItemToggle>();
        _photonView = GetComponent<PhotonView>();
        _physGrabObject = GetComponent<PhysGrabObject>();

        for (int i = 0; i < _songs.Count; i++)
        {
            if (i < _particles.Count)
            {
                _songParticleMap[_songs[i]] = _particles[i];
            }
        }
    }

    private void Update()
    {
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            if (_physGrabObject.grabbed)
            {
                audioSource.volume = 1.0f;
                FirstGrab();
            }
            else if (!_physGrabObject.grabbed && _isPlaying)
            {
                audioSource.volume = 0.4f;
            }
        }
    }
    private void FirstGrab()
    {
        if (_isFirstGrab)
        {
            _toggle.toggleState = true;
            _isFirstGrab = false;
        }
        else
        {
            ToggleAudio();
        }
    }
    private void ToggleAudio()
    {
        if (_toggle.toggleState)
        {
            _photonView.RPC("PlayRandomSongRPC", RpcTarget.All);
        }
        else if (!_toggle.toggleState)
        {
            _isPlaying = false;
            StopParticles();
            audioSource.Stop();
        }
    }
    [PunRPC]
    private void ToggleParticles()
    {
        foreach (var particle in _particles)
        {
            particle.Stop();
        }
        if (_toggle.toggleState && _currentSongIndex < _particles.Count)
        {
            _particles[_currentSongIndex].Play();
        }
    }
    [PunRPC]
    private void PlaySong(string songName)
    {
        AudioClip song = _songs.FirstOrDefault(s => s.name == songName);
        if (_songParticleMap.ContainsKey(song))
        {
            _songParticleMap[song].Play();
        }
    }
    private void StopParticles()
    {
        foreach (ParticleSystem particle in _particles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    [PunRPC]
    private void PlayRandomSongRPC()
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            int randomIndex = Random.Range(0, _songs.Count);
            AudioClip selectedSong = _songs[randomIndex];

            audioSource.clip = selectedSong;
            audioSource.Play();

            _photonView.RPC("PlaySong", RpcTarget.All, selectedSong.name);

            _currentSongIndex = randomIndex;

            _photonView.RPC("ToggleParticles", RpcTarget.All);
        }
    }
}
