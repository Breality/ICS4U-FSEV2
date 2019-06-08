namespace Passer {
    using Humanoid.Tracking;

    public class WindowsMRHmdComponent : SensorComponent {
#if hWINDOWSMR && UNITY_2017_2_OR_NEWER && UNITY_WSA_10_0

        public override void UpdateComponent() {
            WindowsMRDevice.SensorState hmdState = WindowsMRDevice.GetHmdState();

            if (!hmdState.tracked) {
                status = Status.Present;
                return;
            }

            transform.position = trackerTransform.TransformPoint(hmdState.position);
            transform.rotation = trackerTransform.rotation * hmdState.rotation;

            positionConfidence = hmdState.positionConfidence;
            rotationConfidence = hmdState.rotationConfidence;

            status = Status.Tracking;
            gameObject.SetActive(true);

            FuseWithUnityCamera();
        }

        protected virtual void FuseWithUnityCamera() {
            Vector3 deltaPos = Camera.main.transform.position - transform.position;

            trackerTransform.position += deltaPos;
        }
#endif
    }
}