using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
#endif

namespace Passer {
    using Humanoid.Tracking;

    [System.Serializable]
    public class UnityVRHead : UnityHeadSensor {

        public UnityVRHead() {
            enabled = false;
        }

        public override string name {
            get { return "First Person Camera"; }
        }

        private Transform unityVRroot;
        public Transform GetRoot(HumanoidControl humanoid) {
            if (unityVRroot == null) {
                GameObject realWorld = HumanoidControl.GetRealWorld(humanoid.transform);
                unityVRroot = realWorld.transform.Find(UnityVRDevice.trackerName);
                if (unityVRroot == null)
                    CreateUnityVRRoot(realWorld.transform);
            }
            return unityVRroot;
        }

        public Transform cameraTransform;
        public Camera camera;

        public new Tracker tracker;

        #region Start
        public override void Start(HumanoidControl humanoid, Transform targetTransform) {
            base.Start(humanoid, targetTransform);

            camera = headTarget.GetComponentInChildren<Camera>();
            if (enabled) {
                CreateUnityVRRoot();
                camera = CheckCamera(headTarget);

                if (camera == null && cameraTransform != null) {
                    cameraTransform.gameObject.SetActive(true);
                    camera = headTarget.GetComponentInChildren<Camera>();
                }
                else if (camera != null) {
                    cameraTransform = camera.transform;
                }

                CheckCameraLocation();
            }
            else if (camera != null) {
                camera.gameObject.SetActive(false);
            }
        }

        public void CheckCameraLocation() {
            if (UnityVRDevice.xrDevice != UnityVRDevice.XRDeviceType.None) {
                DetachCamera();
            }
            else if (headTarget.head.bone.transform != null) {
                cameraTransform.SetParent(headTarget.head.bone.transform, true);
                cameraTransform.rotation = headTarget.head.target.transform.rotation;
                cameraTransform.position = headTarget.head.target.transform.position + headTarget.head.target.transform.rotation * headTarget.head2eyes;
            }
        }

        private void DetachCamera() {
            unityVRroot.rotation = headTarget.transform.rotation;

            cameraTransform.SetParent(unityVRroot, true);

            if (UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.Oculus)
                unityVRroot.localPosition = new Vector3(0, OculusDevice.eyeHeight, 0);
            else if (UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.OpenVR || UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.WindowsMR)
                unityVRroot.position = headTarget.humanoid.transform.position;
            else
                unityVRroot.position = headTarget.transform.position;

        }

        private void CreateUnityVRRoot() {
            GameObject realWorld = HumanoidControl.GetRealWorld(headTarget.humanoid.transform);
            unityVRroot = realWorld.transform.Find(UnityVRDevice.trackerName);
            if (unityVRroot == null)
                CreateUnityVRRoot(realWorld.transform);

            unityVRroot.parent = realWorld.transform;
#if hOCULUS
            if (UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.Oculus)
                unityVRroot.localPosition = new Vector3(0, OculusDevice.eyeHeight, 0);
#endif
        }

        private void CreateUnityVRRoot(Transform realWorld) {
            GameObject unityVRrootObject = new GameObject(UnityVRDevice.trackerName);
            unityVRroot = unityVRrootObject.transform;
            unityVRroot.parent = realWorld.transform;
        }

        public Transform GetCameraTransform() {
            if (cameraTransform != null)
                return cameraTransform;

            Camera camera = headTarget.GetComponentInChildren<Camera>();
            if (enabled) {
                camera = CheckCamera(headTarget);

                if (camera == null)
                    camera = headTarget.GetComponentInChildren<Camera>();
            }
            if (camera == null)
                return null;

            return camera.transform;
        }

        public static Camera GetCamera(HeadTarget headTarget) {
            if (headTarget.unityVRHead.camera != null)
                return headTarget.unityVRHead.camera;

            Camera camera = headTarget.GetComponentInChildren<Camera>();
            if (headTarget.unityVRHead.enabled) {
                camera = CheckCamera(headTarget);

                if (camera == null)
                    camera = headTarget.GetComponentInChildren<Camera>();

            }
            return camera;
        }

        public static Camera CheckCamera(HeadTarget headTarget) {
            if (headTarget.unityVRHead.enabled) {
                return AddCamera(headTarget);

            }
            else {
                RemoveCamera(headTarget);
                return null;
            }
        }

        public static Camera AddCamera(HeadTarget headTarget) {
            Camera camera = headTarget.transform.GetComponentInChildren<Camera>();
            if (camera == null) {
                Vector3 eyePosition = headTarget.GetEyePosition();

                GameObject cameraObj = new GameObject("First Person Camera");
                camera = cameraObj.AddComponent<Camera>();
                camera.tag = "MainCamera";

                camera.nearClipPlane = 0.1F;

                camera.transform.SetParent(headTarget.transform, false);
                camera.transform.position = eyePosition;
                //if (headTarget.humanoid != null)
                    camera.transform.rotation = Quaternion.Euler(0, headTarget.humanoid.hipsTarget.hips.target.transform.eulerAngles.y, 0);
                //else
                //    camera.transform.rotation = Quaternion.Euler(0, headTarget.iControl.transform.eulerAngles.y, 0);

                cameraObj.AddComponent<AudioListener>();
            }

            if (headTarget.collisionFader)
                AddScreenFader(camera);
            else
                RemoveScreenFader(camera.transform);

            return camera;
        }
        public static Camera AddCamera(Transform targetTransform) {
            Camera camera = targetTransform.GetComponentInChildren<Camera>();
            if (camera == null) {
                GameObject cameraObj = new GameObject("First Person Camera");
                camera = cameraObj.AddComponent<Camera>();
                camera.tag = "MainCamera";

                camera.nearClipPlane = 0.1F;

                camera.transform.SetParent(targetTransform, false);
                camera.transform.localPosition = Vector3.zero;
                camera.transform.rotation = Quaternion.Euler(0, targetTransform.eulerAngles.y, 0);

                cameraObj.AddComponent<AudioListener>();
            }
            AddScreenFader(camera);

            return camera;
        }

        public static void RemoveCamera(HeadTarget headTarget) {
            RemoveCamera(headTarget.transform);
        }
        public static void RemoveCamera(Transform targetTransform) {
            Camera camera = targetTransform.GetComponentInChildren<Camera>();
            if (camera != null) {
                if (Application.isPlaying)
                    Object.Destroy(camera.gameObject);
                else
                    Object.DestroyImmediate(camera.gameObject);
            }
        }

        private static void AddScreenFader(Camera camera) {
            if (camera == null)
                return;

            Transform planeTransform = camera.transform.Find("Fader");
            if (planeTransform != null)
                return;

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.name = "Fader";
            plane.transform.parent = camera.transform;
            plane.transform.localEulerAngles = new Vector3(-90, 0, 0);
            plane.transform.localPosition = new Vector3(0, 0, camera.nearClipPlane + 0.01F);

            Renderer renderer = plane.GetComponent<Renderer>();
            if (renderer != null) {
                Shader fadeShader = Shader.Find("Standard");
                Material fadeMaterial = new Material(fadeShader);
                fadeMaterial.name = "FadeMaterial";
                fadeMaterial.SetFloat("_Mode", 2);
                fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                fadeMaterial.SetInt("_ZWrite", 0);
                fadeMaterial.DisableKeyword("_ALPHATEST_ON");
                fadeMaterial.EnableKeyword("_ALPHABLEND_ON");
                fadeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                fadeMaterial.renderQueue = 3000;
                Color color = Color.black;
                color.a = 0.0F;
                fadeMaterial.SetColor("_Color", new Color(0, 0, 0, 0));
                renderer.material = fadeMaterial;
                renderer.enabled = false;
            }

            Collider c = plane.GetComponent<Collider>();
            Object.DestroyImmediate(c);
        }

        private static void RemoveScreenFader(Transform cameraTransform) {
            if (cameraTransform == null)
                return;

            Transform plane = cameraTransform.Find("Fader");
            if (plane == null)
                return;

            Object.DestroyImmediate(plane.gameObject);
        }
        #endregion

        #region Update
        public override void Update() {
            if (!enabled)
                return;

            UpdateTargetTransform();
        }

        protected override void UpdateTargetTransform() {
            if (UnityVRDevice.xrDevice == UnityVRDevice.XRDeviceType.None)
                UpdateCamera();
        }

        private void UpdateCamera() {
            // we may want to have smoothing here...
            cameraTransform.rotation = headTarget.head.target.transform.rotation;
            cameraTransform.position = headTarget.head.target.transform.position + headTarget.head.target.transform.rotation * headTarget.head2eyes;
        }
        #endregion
    }
}