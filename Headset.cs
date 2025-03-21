using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

    public class Headset : MonoBehaviour
    {
        public UnityEvent playRandomMusic;

        private ItemToggle toggle;

        private void Start ()
        {
            toggle = GetComponent<ItemToggle>();
        }
    }
