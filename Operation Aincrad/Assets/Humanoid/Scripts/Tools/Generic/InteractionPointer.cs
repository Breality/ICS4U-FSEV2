using UnityEngine;
using UnityEngine.EventSystems;

namespace Passer {
    public class InteractionPointer : MonoBehaviour {
        public bool active;

        public float timedClick;
        private bool hasClicked = false;

        public GameObject focusPointObj;

        public GameObject objectInFocus;

        protected Interaction inputModule;
        protected int interactionID;
        protected LineRenderer lineRenderer;

        public enum RayType {
            Straight,
            Bezier,
            Gravity,
            SphereCast,
        }
        public RayType rayType = RayType.Straight;
        public float maxDistance = 3;
        public float resolution = 0.2F;
        public float speed = 3;

        private int nCurveSegments;
        private Vector3[] curvePoints;

        public enum PointerType {
            FocusPoint,
            Ray
        }
        public static InteractionPointer Add(Transform parentTransform, PointerType pointerType = PointerType.Ray) {
            GameObject pointerObj = new GameObject("Interaction Pointer");
            pointerObj.transform.SetParent(parentTransform);
            pointerObj.transform.localPosition = new Vector3(0.2039F, -0.0223F, 0.0092F);
            pointerObj.transform.localRotation = Quaternion.Euler(-180, -90, -180);

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
            }
            else {
                LineRenderer pointerRay = focusPointObj.AddComponent<LineRenderer>();
                pointerRay.startWidth = 0.01F;
                pointerRay.endWidth = 0.01F;
                pointerRay.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                pointerRay.receiveShadows = false;
                pointerRay.useWorldSpace = false;
            }

            InteractionPointer pointer = pointerObj.AddComponent<InteractionPointer>();
            pointer.focusPointObj = focusPointObj;
            pointer.rayType = RayType.Straight;
            return pointer;
        }

        #region Init
        protected virtual void Awake() {
            Transform rootTransform = this.transform.root;
            inputModule = rootTransform.GetComponent<Interaction>();
            if (inputModule == null) {
                inputModule = rootTransform.gameObject.AddComponent<Interaction>();
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
        }

        protected virtual void Start() {
            nCurveSegments = Mathf.CeilToInt(maxDistance / resolution);
            curvePoints = new Vector3[nCurveSegments + 1];
        }
        #endregion

        #region Update
        protected virtual void Update() {
            if (focusPointObj == null)
                return;

            inputModule.ActivatePointing(interactionID, active);
            //inputModule.Process();

            if (inputModule.IsPointing(interactionID)) {
                focusPointObj.SetActive(true);

                if (rayType == RayType.SphereCast) {
                    UpdateSpherecast();
                }
                else if (lineRenderer != null) {
                    lineRenderer.enabled = true;
                    switch (rayType) {
                        case RayType.Straight:
                            UpdateStraight();
                            break;
                        case RayType.Bezier:
                            UpdateBezier();
                            break;
                        case RayType.Gravity:
                            UpdateGravity();
                            break;
                    }
                }
                else {
                    focusPointObj.transform.position = inputModule.GetFocusPoint(interactionID);
                    focusPointObj.transform.rotation = inputModule.GetFocusRotation(interactionID);
                }
                objectInFocus = inputModule.GetFocusObject(interactionID);

                if (timedClick != 0) {
                    if (!hasClicked && inputModule.IsTimedClick(interactionID)) {
                        Click(true);
                        hasClicked = true;
                        Click(false);
                    }
                }

            }
            else {
                focusPointObj.SetActive(false);
                focusPointObj.transform.position = transform.position;
                objectInFocus = null;
                hasClicked = false;
            }

            Focus();
        }

        public void SetRayDirection(Vector3 direction) {
            inputModule.SetPointingDirection(interactionID, direction);
        }

        #region Straight
        private void UpdateStraight() {
            focusPointObj.transform.position = inputModule.GetFocusPoint(interactionID);
            focusPointObj.transform.rotation = inputModule.GetFocusRotation(interactionID);

            Vector3 endPosition = focusPointObj.transform.InverseTransformPoint(transform.position);

#if UNITY_5_6_OR_NEWER
            lineRenderer.positionCount = 2;
#else
            lineRenderer.numPositions = 2;
#endif
            lineRenderer.SetPosition(0, endPosition);
        }
        #endregion

        #region Bezier
        private void UpdateBezier() {
            Vector3 normal;
            GameObject focusObject = null;
            Vector3[] bezierPositions = UpdateBezierCurve(transform, maxDistance, out normal, out focusObject);

            Vector3 focusPosition = bezierPositions[bezierPositions.Length - 1];
            Quaternion focusRotation = Quaternion.LookRotation(normal, transform.forward);

            inputModule.SetExternalRayCast(interactionID, focusPosition, focusRotation, focusObject);

            focusPointObj.transform.position = focusPosition; // bezierPositions[bezierPositions.Length - 1];
            focusPointObj.transform.rotation = focusRotation; // Quaternion.LookRotation(normal, transform.forward);


            for (int i = 0; i < bezierPositions.Length; i++)
                bezierPositions[i] = focusPointObj.transform.InverseTransformPoint(bezierPositions[i]);

#if UNITY_5_6_OR_NEWER
            lineRenderer.positionCount = bezierPositions.Length;
#else
            lineRenderer.numPositions = bezierPositions.Length;
#endif
            lineRenderer.SetPositions(bezierPositions);
        }


        private float heightLimitAngle = 100f;

        private Vector3 startPosition = Vector3.zero;
        private Vector3 intermediatePosition;
        private Vector3 endPosition;

        public Vector3[] UpdateBezierCurve(Transform transform, float maxDistance, out Vector3 normal, out GameObject focusObject) {
            float distance = maxDistance;

            float attachedRotation = Vector3.Dot(Vector3.up, transform.forward);
            if ((attachedRotation * 100f) > heightLimitAngle) {
                float controllerRotationOffset = 1f - (attachedRotation - (heightLimitAngle / 100f));
                distance = (maxDistance * controllerRotationOffset) * controllerRotationOffset;
            }

            intermediatePosition = Vector3.forward * distance;

            RaycastHit rayHit;
            if (Physics.Raycast(transform.TransformPoint(intermediatePosition), Vector3.down, out rayHit, 10)) {
                normal = rayHit.normal;
                focusObject = rayHit.transform.gameObject;
                endPosition = transform.InverseTransformPoint(rayHit.point);

                for (int i = 0; i <= nCurveSegments; i++)
                    curvePoints[i] = GetPoint(i / (float)nCurveSegments, transform);

            }
            else {
                normal = Vector3.up;
                focusObject = null;
            }

            return curvePoints;
        }

        public Vector3 GetPoint(float t, Transform transform) {
            Vector3 localPoint = GetBezierPoint(startPosition, intermediatePosition, endPosition, t);
            return transform.TransformPoint(localPoint);
        }

        public Vector3 GetVelocity(float t, Transform transform) {
            Vector3 localVelocity = GetBezierFirstDerivative(startPosition, intermediatePosition, endPosition, t);
            return transform.TransformPoint(localVelocity) - transform.position;
        }

        public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        public static Vector3 GetBezierFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            return
                2f * (1f - t) * (p1 - p0) +
                2f * t * (p2 - p1);
        }
        //}
        #endregion

        #region Gravity
        private void UpdateGravity() {
            Vector3 normal;
            GameObject focusObject = null;
            UpdateGravityCurve(transform, speed, out normal, out focusObject);

            Vector3 focusPosition = curvePoints[curvePoints.Length - 1];
            Quaternion focusRotation = Quaternion.LookRotation(normal, transform.forward);

            inputModule.SetExternalRayCast(interactionID, focusPosition, focusRotation, focusObject);

            focusPointObj.transform.position = focusPosition;// curvePoints[curvePoints.Length - 1];
            focusPointObj.transform.rotation = focusRotation;// Quaternion.LookRotation(normal, transform.forward);

            for (int i = 0; i < curvePoints.Length; i++)
                curvePoints[i] = focusPointObj.transform.InverseTransformPoint(curvePoints[i]);

#if UNITY_5_6_OR_NEWER
            lineRenderer.positionCount = curvePoints.Length;
#else
            lineRenderer.numPositions = curvePoints.Length;
#endif
            lineRenderer.SetPositions(curvePoints);
        }

        public void UpdateGravityCurve(Transform transform, float forwardSpeed, out Vector3 normal, out GameObject hitObject) {
            curvePoints[0] = transform.position;
            Vector3 segVelocity = transform.forward * forwardSpeed;
            normal = Vector3.up;
            hitObject = null;

            for (int i = 1; i < nCurveSegments + 1; i++) {
                if (hitObject != null) {
                    curvePoints[i] = curvePoints[i - 1];
                    continue;
                }
                // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
                float segTime = (segVelocity.sqrMagnitude != 0) ? resolution / segVelocity.magnitude : 0;

                // Add velocity from gravity for this segment's timestep
                segVelocity = segVelocity + Physics.gravity * segTime;

                // Check to see if we're going to hit a physics object
                RaycastHit hit;
                if (Physics.Raycast(curvePoints[i - 1], segVelocity.normalized, out hit, resolution)) {
                    normal = hit.normal;
                    hitObject = hit.transform.gameObject;

                    // set next position to the position where we hit the physics object
                    curvePoints[i] = curvePoints[i - 1] + segVelocity.normalized * hit.distance;
                }
                // If our raycast hit no objects, then set the next position to the last one plus v*t
                else {
                    curvePoints[i] = curvePoints[i - 1] + segVelocity * segTime;
                }
            }
        }
        #endregion

        #region Spherecast
        public float radius = 0.1F;
        private void UpdateSpherecast() {
            GameObject focusObject = null;

            RaycastHit hit;
            if (Physics.SphereCast(transform.position, radius, transform.forward, out hit, maxDistance)) {
                focusObject = hit.transform.gameObject;

                focusPointObj.transform.position = hit.point;
                focusPointObj.transform.rotation = Quaternion.LookRotation(hit.normal, transform.forward);
            }
            else {
                focusPointObj.transform.position = transform.position + transform.forward * maxDistance;
                focusPointObj.transform.rotation = Quaternion.identity;
            }


            inputModule.SetExternalRayCast(interactionID, focusPointObj.transform.position, focusPointObj.transform.rotation, focusObject);
        }
        #endregion

        #region Events
        public InputEvent clickInput = new InputEvent();
        public virtual void Click(bool clicking) {
            clickInput.boolValue = clicking;

            if (clicking) {
                inputModule.ClickDown(interactionID);
            }
            else
                inputModule.ClickUp(interactionID);
        }

        public InputEvent activeInput = new InputEvent();
        public virtual void Activation(bool _active) {
            active = _active;
            activeInput.boolValue = _active;
        }

        public InputEvent focusInput = new InputEvent();
        public void Focus() {
            focusInput.boolValue = (objectInFocus != null);
        }
        #endregion

        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected() {
            if (rayType == RayType.SphereCast) {
                Gizmos.color = Color.green;

                Gizmos.DrawWireSphere(transform.position, radius);
                Gizmos.DrawRay(transform.position + transform.up * radius, transform.forward * maxDistance);
                Gizmos.DrawRay(transform.position - transform.up * radius, transform.forward * maxDistance);
                Gizmos.DrawRay(transform.position + transform.right * radius, transform.forward * maxDistance);
                Gizmos.DrawRay(transform.position - transform.right * radius, transform.forward * maxDistance);
                Gizmos.DrawWireSphere(transform.position + transform.forward * maxDistance, radius);
            }
        }
        #endregion
    }
}
