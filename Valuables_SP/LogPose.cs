using Photon.Pun;
using UnityEngine;

namespace SteampunkItems.Valuables_SP;

public class LogPose : MonoBehaviour
{
    private PhotonView _photonView;
    private PhysGrabObject _physGrabObject;
    private Transform currentTarget;

    public Transform needle;


    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _physGrabObject = GetComponent<PhysGrabObject>();
    }
    private void Update()
    {

    }
    private void SetAndResetPos()
    {
        if (_physGrabObject.grabbed)
        {
            ExtractionPoint extractionPoint = SemiFunc.ExtractionPointGetNearest(needle.position); //rotation
            if ((bool)extractionPoint)
            {
                currentTarget = extractionPoint.transform;
                SetTarget();
            }
        }
        else
        {

        }
    }
    private void SetTarget()
    {

    }
}
