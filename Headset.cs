using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        if (_physGrabObject.grabbed)
        {
            ToggleAudio();
        }
        else
        {
            _photonView.RPC("StopParticlesRPC", RpcTarget.All);
        }
    }

    private void ToggleAudio()
    {
        if (_toggle.toggleState)
        {
            PlayRandomSongRPC();
        }
        else
        {
            audioSource.Stop();
            _isPlaying = false;
        }
    }

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
    private void PlayParticleRPC(string songName)
    {
        AudioClip song = _songs.FirstOrDefault(s => s.name == songName);
        if (_songParticleMap.ContainsKey(song))
        {
            _songParticleMap[song].Play();
        }
    }

    [PunRPC]
    private void StopParticlesRPC()
    {
        foreach (ParticleSystem particle in _particles)
        {
            particle.Stop();
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

            _photonView.RPC("PlayParticleRPC", RpcTarget.All, selectedSong.name);

            _currentSongIndex = randomIndex;

            ToggleParticles();
        }
    }
}
