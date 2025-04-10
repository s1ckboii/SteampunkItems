using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class Headset : MonoBehaviour
{
    public AudioSource audioSource;
    public List<ParticleSystem> _particles = [];
    public List<AudioClip> _songs = [];

    private PhotonView _photonView;
    private PhysGrabObject _physGrabObject;

    private int _currentSongIndex;
    private ItemToggle _toggle;
    private bool _isPlaying;
    private bool _isFirstGrab = true;

    private void Awake()
    {
        _toggle = GetComponent<ItemToggle>();
        _photonView = GetComponent<PhotonView>();
        _physGrabObject = GetComponent<PhysGrabObject>();
    }
    private void Update()
    {
        if (_physGrabObject.grabbed)
        {
            if (_physGrabObject.grabbedLocal)
            {
                audioSource.volume = 0.5f;
            }
            if (_isFirstGrab)
            {
                _toggle.toggleState = true;
                _isFirstGrab = false;
            }
            ToggleAudio();
        }
        else
        {
            audioSource.volume -= Time.deltaTime * 0.25f;
            audioSource.volume = Mathf.Max(audioSource.volume, 0.1f);
        }

    }


    private void ToggleAudio()
    {
        if (_toggle.toggleState)
        {
            int randomIndex = Random.Range(0, _songs.Count);
            if (SemiFunc.IsMultiplayer())
            {
                _photonView.RPC("PlaySongRPC", RpcTarget.All, randomIndex);
            }
            else
            {
                PlaySongRPC(randomIndex);
            }
        }
        else if (!_toggle.toggleState)
        {
            _isPlaying = false;
            StopParticles();
            audioSource.Stop();
        }
    }
    private void PlayParticles(int songIndex)
    {
        StopParticles();

        if (songIndex < _particles.Count)
        {
            _particles[songIndex].Play();
        }
    }
    private void StopParticles()
    {
        foreach (var particle in _particles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    [PunRPC]
    private void PlaySongRPC(int songIndex)
    {
        if (!_isPlaying)
        {
            _isPlaying = true;

            audioSource.clip = _songs[songIndex];
            audioSource.Play();

            _currentSongIndex = songIndex;

            PlayParticles(songIndex);
        }
    }
}
