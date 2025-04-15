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
            _mirrorTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
            {
                useMipMap = false,
                antiAliasing = 2,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "MirrorRT_" + gameObject.name
            };
            _mirrorTexture.Create();

            _playerCam = GameDirector.instance.MainCamera;

            _mirrorMaterial.SetTexture("_ReflectionTex", _mirrorTexture);
            _mirrorMaterial.SetFloat("_ReflectIntensity", 1);

            _mirrorCam.nearClipPlane = 0.05f;
            _mirrorCam.farClipPlane = 100f;

            _mirrorCam.targetTexture = _mirrorTexture;

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
            /*
            Vector3 reflectedPos = Vector3.Reflect(_playerCam.transform.position - _mirrorPlane.position, _mirrorPlane.forward) + _mirrorPlane.position;
            Vector3 reflectedForward = Vector3.Reflect(_playerCam.transform.forward, _mirrorPlane.transform.forward);
            Vector3 reflectedUp = Vector3.Reflect(_playerCam.transform.up, _mirrorPlane.forward);

            _mirrorCam.transform.SetPositionAndRotation(reflectedPos, Quaternion.LookRotation(reflectedForward, reflectedUp));
            */
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