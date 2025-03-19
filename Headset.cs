using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

namespace SteampunkItems
{
    public class Headset : MonoBehaviour
    {
        public UnityEvent playRandomMusic;

        private ItemToggle toggle;

        private void Start ()
        {
            toggle = GetComponent<ItemToggle>();
        }
    }
}
