using UnityEngine;

namespace SteampunkItems.Valuables_SP
{
    public class Mirror : MonoBehaviour
    {
        public Transform _mirrorPlane;
        public Material _mirrorMaterial;
        public MeshRenderer _mirrorRenderer;
        public Camera _mirrorCam;

        private Camera _playerCam;
        private RenderTexture _mirrorTexture;
        private Transform _rendererTransform;
        private Vector3 _originalRendererPos;
        private bool _rendererMoved = false;

        public void Start()
        {
            _mirrorTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            _mirrorTexture.useMipMap = false;
            _mirrorTexture.antiAliasing = 2;
            _mirrorTexture.wrapMode = TextureWrapMode.Clamp;
            _mirrorTexture.filterMode = FilterMode.Bilinear;
            _mirrorTexture.name = "MirrorRT_" + gameObject.name;
            _mirrorTexture.Create();

            _playerCam = GameDirector.instance.MainCamera;

            _mirrorMaterial.SetTexture("_ReflectionTex", _mirrorTexture);
            _mirrorMaterial.SetFloat("_ReflectIntensity", 1);

            _mirrorCam.targetTexture = _mirrorTexture;

            _mirrorCam.nearClipPlane = 0.05f;
            _mirrorCam.farClipPlane = 100f;

            _rendererTransform = _mirrorRenderer.transform;
        }

        public void LateUpdate()
        {
            if (!IsVisibleToCamera(_mirrorRenderer, _playerCam)) return;

            if (!_rendererMoved)
            {
                _originalRendererPos = _rendererTransform.position;
                _rendererTransform.position = Vector3.one * 9999f;
                _rendererMoved = true;
            }

            Vector3 reflectedPos = ReflectPos(_playerCam.transform.position);
            Vector3 reflectedForward = ReflectDir(_playerCam.transform.forward);
            Vector3 reflectedUp = ReflectDir(_playerCam.transform.up);

            _mirrorCam.transform.SetPositionAndRotation(reflectedPos, Quaternion.LookRotation(reflectedForward, reflectedUp));

            GL.invertCulling = true;
            _mirrorCam.Render();
            GL.invertCulling = false;
        }

        public void OnPostRender()
        {
            if (_rendererMoved)
            {
                _rendererTransform.position = _originalRendererPos;
                _rendererMoved = false;
            }
        }

        public Vector3 ReflectPos(Vector3 pos)
        {
            Vector3 normal = _mirrorPlane.forward;
            Vector3 toPos = pos - _mirrorPlane.position;
            return Vector3.Reflect(toPos, normal);
        }

        public Vector3 ReflectDir(Vector3 dir)
        {
            Vector3 normal = _mirrorPlane.forward;
            return Vector3.Reflect(dir, normal);
        }

        public bool IsVisibleToCamera(Renderer rend, Camera cam)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, rend.bounds);
        }

        public void OnDestroy()
        {
            if (_mirrorTexture != null)
            {
                _mirrorTexture.Release();
                Destroy(_mirrorTexture);
            }
        }
    }
}