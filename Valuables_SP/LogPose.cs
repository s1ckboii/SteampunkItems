using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace SteampunkItems.Valuables_SP;

public class LogPose : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    private PhotonView photonView;
    private PhysGrabObject physGrabObject;

    private ExtractionPoint extractionPoint;

    private Vector3 startPos;
    private Vector3 endPos;

    private bool activeExtPoint;
    private float guideTimer;
    private float particlesPerSecond = 100f;
    private float distance;


    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        physGrabObject = GetComponent<PhysGrabObject>();
    }
    private void Update()
    {
        if (physGrabObject.grabbed)
        {
            
        }
    }
    private IEnumerator EmitParticles()
    {
        float timer = 0f;
        while (timer < guideTimer)
        {
            startPos = base.transform.position;
            extractionPoint = SemiFunc.ExtractionPointGetNearest(startPos);
            endPos = extractionPoint.transform.position;
            distance = Vector3.Distance(startPos, endPos);
            int steps = Mathf.CeilToInt(particlesPerSecond * Time.deltaTime);

            for (int i = 0; i < steps; ++i)
            {
                float t = i / (float)steps;
                Vector3 pos = Vector3.Lerp(startPos, endPos, t);
                EmitAtPos(pos, t);
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private void EmitAtPos(Vector3 pos, float t)
    {
        var emitParams = new ParticleSystem.EmitParams
        {
            position = pos,
            startLifetime = 1f,
            startColor = new Color(1f, 1f, 1f, 1f - t),
            velocity = Vector3.zero
        };
        ps.Emit(emitParams, 1);
    }
}
