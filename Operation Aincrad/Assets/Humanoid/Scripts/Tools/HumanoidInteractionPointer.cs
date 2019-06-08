using UnityEngine;
using UnityEngine.EventSystems;

namespace Passer.Humanoid {

    public class HumanoidInteractionPointer : InteractionPointer {

        public static new InteractionPointer Add(Transform parentTransform, PointerType pointerType = PointerType.Ray) {
            GameObject pointerObj = new GameObject("Interaction Pointer");
            pointerObj.transform.SetParent(parentTransform);
            pointerObj.transform.localPosition = Vector3.zero;
            pointerObj.transform.localRotation = Quaternion.identity;

            GameObject focusPointObj = new GameObject("FocusPoint");
            focusPointObj.transform.SetParent(pointerObj.transform);
            focusPointObj.transform.localPosition = Vector3.zero;
            focusPointObj.transform.localRotation = Quaternion.identity;

            if (pointerType == PointerType.FocusPoint) {
                GameObject focusPointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                focusPointSphere.transform.SetParent(focusPointObj.transform);
                focusPointSphere.transform.localPosition = Vector3.zero;
                focusPointSphere.transform.localRotation = Quaternion.identity;
                focusPointSphere.transform.localScale = Vector3.one * 0.1F;
                Collider collider = focusPointSphere.GetComponent<Collider>();
                DestroyImmediate(collider, true);
            } else {
                LineRenderer pointerRay = focusPointObj.AddComponent<LineRenderer>();
                pointerRay.startWidth = 0.01F;
                pointerRay.endWidth = 0.01F;
                pointerRay.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                pointerRay.receiveShadows = false;
                pointerRay.useWorldSpace = false;
            }

            InteractionPointer pointer = pointerObj.AddComponent<HumanoidInteractionPointer>();
            pointer.focusPointObj = focusPointObj;
            pointer.rayType = RayType.Straight;
            return pointer;
        }

        private HeadTarget headTarget;

        protected override void Awake() {
            base.Awake();
            HumanoidControl humanoid = this.transform.root.GetComponentInChildren<HumanoidControl>();
            if (humanoid == null)
                base.Awake();

            else {
                inputModule = humanoid.GetComponent<Interaction>();
                if (inputModule == null) {
                    inputModule = humanoid.gameObject.AddComponent<Interaction>();
                }

                interactionID = inputModule.CreateNewInteraction(transform, timedClick);

                if (focusPointObj == null) {
                    focusPointObj = new GameObject("Focus Point");
                    focusPointObj.transform.parent = transform;
                }

                lineRenderer = focusPointObj.GetComponent<LineRenderer>();
                if (lineRenderer != null) {
                    lineRenderer.useWorldSpace = false;
                    lineRenderer.SetPosition(1, Vector3.zero);
                }

                headTarget = transform.parent.GetComponent<HeadTarget>();
            }
        }

        protected override void Update() {
            base.Update();


            if (headTarget != null) {
#if hFACE
                transform.rotation = Quaternion.LookRotation(headTarget.face.gazeDirection);
#endif
            }
        }

        public override void Activation(bool _active) {
            base.Activation(_active);
        }
        public override void Click(bool clicking) {
            base.Click(clicking);
        }
    }
}