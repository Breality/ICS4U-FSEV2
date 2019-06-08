using UnityEngine;
using UnityEngine.EventSystems;

namespace Passer {

    public enum InputDeviceIDs {
        Head,
        LeftHand,
        RightHand,
        Controller
    }

    public class InteractionEventData : PointerEventData {
        public InteractionEventData(EventSystem eventSystem) : base(eventSystem) { }

        public InputDeviceIDs inputDevice;
        public Transform transform;
    }

    public class Interaction : BaseInputModule {

        private const int maxInteractions = 8;
        private int nInteractions = 3;

        private enum PointerType {
            Point,
            Touch
        }

        private class InteractionPointer {
            public Transform pointerTransform;
            public Vector3 localPointingDirection;
            public InteractionEventData data;
            public Vector2 previousPosition;

            public bool focusingEnabled = false;
            public bool focusing;
            public GameObject focusObject;
            public GameObject previousFocusObject;
            public Vector3 focusPosition;
            public Quaternion focusRotation;
            public float focusStart;
            public float focusTimeToTouch = 2;

            public bool externalRayCast = false;

            public bool clicking = false;
            public GameObject touchedObject;
            public Vector3 touchPosition;

            public ControllerSide controllerInputSide;
            public Controller.Button controllerButton = Controller.Button.None;

            public bool hasClicked;

            public PointerType type;

            public InteractionPointer(PointerType type, Transform pointerTransform, EventSystem eventSystem) {
                this.type = type;
                this.pointerTransform = pointerTransform;
                this.data = new InteractionEventData(eventSystem);
            }

            public void ProcessFocus() {
                if (!focusingEnabled)
                    return;

                if (focusObject != previousFocusObject) {
                    if (previousFocusObject != null) {
                        ExecuteEvents.ExecuteHierarchy(previousFocusObject, data, ExecuteEvents.pointerExitHandler);
                        ExecuteEvents.ExecuteHierarchy(previousFocusObject, data, ExecuteEvents.deselectHandler);
                    }
                    if (focusObject != null) {
                        ExecuteEvents.ExecuteHierarchy(focusObject, data, ExecuteEvents.pointerEnterHandler);
                        ExecuteEvents.ExecuteHierarchy(focusObject, data, ExecuteEvents.selectHandler);
                        focusStart = Time.time;
                        hasClicked = false;
                        focusing = true;
                    }
                    previousFocusObject = focusObject;
                }
            }

            public void ProcessNoFocus() {
                if (previousFocusObject != null) {
                    ExecuteEvents.ExecuteHierarchy(previousFocusObject, data, ExecuteEvents.pointerExitHandler);
                    ExecuteEvents.ExecuteHierarchy(previousFocusObject, data, ExecuteEvents.deselectHandler);
                    focusing = false;
                    // no focus = no clicking
                    clicking = false;
                    previousFocusObject = null;
                }
            }

            public void ProcessTouch() {
                if (!focusing) {
                    touchedObject = data.pointerCurrentRaycast.gameObject;
                    if (touchedObject == null) // object is a 3D object, as we do not use Physicsraycaster, use the focusObject
                        touchedObject = focusObject;

                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerEnterHandler);
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.selectHandler);

                    focusing = true;
                    return;

                }
                if (!clicking) { // first activation
                    touchedObject = data.pointerCurrentRaycast.gameObject;
                    if (touchedObject == null) // object is a 3D object, as we do not use Physicsraycaster, use the focusObject
                        touchedObject = focusObject;

                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerDownHandler);

                    clicking = true;
                }
                else { // we were already touching
                    if (data.delta.sqrMagnitude > 0) { // moved finger during touch
                        GameObject pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(data.pointerCurrentRaycast.gameObject);
                        if (!data.dragging) { // we were not dragging yet
                            if (pointerDrag != null) { // start dragging only where there is something to drag
                                data.pointerDrag = pointerDrag;
                                data.dragging = ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.beginDragHandler);
                            }
                        }
                        else { // still dragging
                            ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.dragHandler);
                        }

                    }
                    else { // finger did not move
                        if (data.dragging) { // we were dragging
                            ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.endDragHandler);
                            data.dragging = false;
                        }
                    }
                }
            }

            public void ProcessNoTouch() {
                if (clicking) { // We were touching
                    if (data.dragging) {
                        ExecuteEvents.ExecuteHierarchy(data.pointerDrag, data, ExecuteEvents.endDragHandler);
                        data.dragging = false;
                    }
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerClickHandler);
                    clicking = false;
                    touchedObject = null;
                }
                ProcessNoFocus();
            }

            public void ProcessClick() {
                if (hasClicked)
                    return;

                if (touchedObject != null) {
                    ExecuteEvents.ExecuteHierarchy(touchedObject, data, ExecuteEvents.pointerUpHandler);
                    Debug.Log("Pointer Up");
                }
                else {
                    ExecuteEvents.ExecuteHierarchy(focusObject, data, ExecuteEvents.pointerUpHandler);
                    Debug.Log("Pointer Up");
                }
                hasClicked = true;
            }
        }

        private InteractionPointer[] pointers = new InteractionPointer[maxInteractions];

        public int CreateNewInteraction(Transform transform, float timedClick) {
            InteractionPointer pointer = new InteractionPointer(PointerType.Point, transform, eventSystem);
            pointers[nInteractions++] = pointer;

            pointer.focusingEnabled = false;
            pointer.localPointingDirection = Vector3.forward;

            pointer.focusTimeToTouch = timedClick;

            return nInteractions - 1;
        }

        public void ActivatePointing(int inputDevice, bool active) {
            if (!pointers[inputDevice].focusingEnabled && active)
                pointers[inputDevice].focusStart = 0;
            pointers[inputDevice].focusingEnabled = active;
        }

        public bool IsPointing(int inputDevice) {
            return pointers[inputDevice].focusingEnabled;
        }

        public void SetPointingDirection(int inputDevice, Vector3 direction) {
            InteractionPointer pointer = pointers[inputDevice];
            pointer.localPointingDirection = pointer.pointerTransform.InverseTransformDirection(direction);
        }

        public bool IsTimedClick(int inputDevice) {
            InteractionPointer pointer = pointers[inputDevice];

            return
                !pointer.hasClicked &&
                pointer.focusTimeToTouch != 0 &&
                pointer.focusStart > 0 &&
                Time.time - pointer.focusStart > pointer.focusTimeToTouch;
        }

        public void ClickDown(int inputDevice) {
            pointers[inputDevice].ProcessTouch();
        }

        public void ClickUp(int inputDevice) {
            pointers[inputDevice].ProcessNoTouch();
        }

        #region FingerInput
        public void EnableTouchInput(HumanoidControl humanoid, bool isLeft, float autoActivation) {
            if (humanoid.avatarRig == null)
                return;

            if (pointers == null)
                pointers = new InteractionPointer[maxInteractions]; // 0 = left index, 1 = right index, 2 = head, 3 = controller

            Controller controllerInput = Controllers.GetController(0);

            Transform indexFingerDistal = null;
            Transform indexFingerTip = null;

            InteractionPointer pointer = null;
            if (isLeft) {
                indexFingerDistal = humanoid.avatarRig.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
                if (indexFingerDistal != null) {
                    if (indexFingerDistal.childCount == 1)
                        indexFingerTip = indexFingerDistal.GetChild(0);
                    else
                        indexFingerTip = indexFingerDistal;
                    pointer = new InteractionPointer(PointerType.Touch, indexFingerTip, eventSystem);
                    pointers[(int)InputDeviceIDs.LeftHand] = pointer;

                    if (controllerInput != null)
                        pointer.controllerInputSide = controllerInput.left;
                }
            }
            else {
                indexFingerDistal = humanoid.avatarRig.GetBoneTransform(HumanBodyBones.RightIndexDistal);
                if (indexFingerDistal != null) {
                    if (indexFingerDistal.childCount == 1)
                        indexFingerTip = indexFingerDistal.GetChild(0);
                    else
                        indexFingerTip = indexFingerDistal;
                    pointer = new InteractionPointer(PointerType.Touch, indexFingerTip, eventSystem);
                    pointers[(int)InputDeviceIDs.RightHand] = pointer;

                    if (controllerInput != null)
                        pointer.controllerInputSide = controllerInput.right;
                }
            }

            if (indexFingerDistal != null) {
                if (indexFingerDistal.childCount > 0) {
                    indexFingerTip = indexFingerDistal.GetChild(0);
                    pointer.localPointingDirection = indexFingerTip.InverseTransformDirection(indexFingerTip.position - indexFingerDistal.position).normalized;
                }
                else {
                    pointer.localPointingDirection = indexFingerDistal.InverseTransformDirection(indexFingerDistal.position - indexFingerDistal.parent.position).normalized;
                }
            }

            if (pointer != null) {
                pointer.focusTimeToTouch = autoActivation;

                // To support hovering
                pointer.focusingEnabled = true;
            }
        }

        public void ActivateFingerPointing(bool isPointing, bool isLeft) {
            InteractionPointer pointer = isLeft ? pointers[(int)InputDeviceIDs.LeftHand] : pointers[(int)InputDeviceIDs.RightHand];

            if (pointer != null) {
                pointer.focusingEnabled = isPointing; // only focusing when we are pointing
            }
        }

        public void OnFingerTouchStart(bool isLeft, GameObject obj) {
            InteractionPointer pointer = isLeft ? pointers[(int)InputDeviceIDs.LeftHand] : pointers[(int)InputDeviceIDs.RightHand];
            if (pointer == null)
                return;

            pointer.focusObject = obj;
            pointer.data.delta = Vector3.zero;
            pointer.ProcessTouch();
        }

        public void OnFingerTouchEnd(bool isLeft) {
            InteractionPointer pointer = isLeft ? pointers[(int)InputDeviceIDs.LeftHand] : pointers[(int)InputDeviceIDs.RightHand];
            if (pointer == null)
                return;

            pointer.ProcessNoTouch();
        }

        public void HandInteractionActivation(bool isLeft) {
            InteractionPointer pointer = isLeft ? pointers[(int)InputDeviceIDs.LeftHand] : pointers[(int)InputDeviceIDs.RightHand];
            if (pointer == null)
                return;

            pointer.ProcessTouch();
        }
        #endregion

        public Vector3 GetFocusPoint(int inputDeviceID) {
            return pointers[inputDeviceID].focusPosition;
        }
        public Quaternion GetFocusRotation(int inputDeviceID) {
            return pointers[inputDeviceID].focusRotation;
        }

        public GameObject GetFocusObject(int inputDeviceID) {
            if (pointers[(int)inputDeviceID].focusObject != null)
                return pointers[(int)inputDeviceID].focusObject;
            else
                return null;
        }

        public GameObject GetTouchObject(InputDeviceIDs inputDeviceID) {
            InteractionPointer pointer = pointers[(int)inputDeviceID];
            if (pointer == null)
                return null;

            if (pointer.touchedObject != null) {
                return pointer.touchedObject;
            }
            else
                return null;
        }

        public float GetGazeDuration(InputDeviceIDs inputDeviceID) {
            return Time.time - pointers[(int)inputDeviceID].focusStart;
        }

        public void SetExternalRayCast(int inputDeviceID, Vector3 focusPosition, Quaternion focusRotation, GameObject focusObject) {
            InteractionPointer pointer = pointers[inputDeviceID];
            pointer.focusPosition = focusPosition;
            pointer.focusRotation = focusRotation;
            pointer.focusObject = focusObject;
            pointer.externalRayCast = true;
        }

        public override void Process() {
            for (int i = 0; i < pointers.Length; i++) {
                if (pointers[i] != null) {
                    ProcessPointer(pointers[i], (InputDeviceIDs)i);
                }
            }
        }

        private void ProcessPointer(InteractionPointer pointer, InputDeviceIDs inputDeviceID) {
            CastRayFromPointer(pointer, inputDeviceID);

            if (pointer.focusingEnabled && pointer.type == PointerType.Point)
                pointer.ProcessFocus();
            if (pointer.focusObject != null && pointer.focusTimeToTouch != 0 && Time.time - pointer.focusStart > pointer.focusTimeToTouch) { // we are clicking
                pointer.ProcessTouch();
                pointer.ProcessNoTouch();
            }
            else if (pointer.clicking) {
                pointer.ProcessTouch();
            }

            if (pointer.data.pointerCurrentRaycast.gameObject == null) { // no focus
                return;
            }

            pointer.data.pressPosition = pointer.data.position;
            pointer.data.pointerPressRaycast = pointer.data.pointerCurrentRaycast;
            pointer.data.pointerPress = null; //Clear this for setting later
            pointer.data.useDragThreshold = true;

            if (pointer.type == PointerType.Touch) {
                // UI touch without colliders works on the finger tip
                float distance = DistanceTipToTransform(pointer.pointerTransform, pointer.data.pointerCurrentRaycast.gameObject.transform);
                if (distance < 0) { // we are touching
                    pointer.ProcessTouch();
                }
                else if (distance > 0.05F) {
                    pointer.ProcessNoTouch();
                }
                else {
                    pointer.ProcessFocus();
                }
            }
        }

        private void CastRayFromPointer(InteractionPointer pointer, InputDeviceIDs inputDeviceID) {
            pointer.data.Reset();
            pointer.data.inputDevice = inputDeviceID;
            pointer.data.transform = pointer.pointerTransform;
            if (pointer.pointerTransform == null)
                return;

            CastPhysicsRayFromPointer(pointer);
            CastUIRayFromPointer(pointer);

            pointer.data.scrollDelta = Vector2.zero;
            if (pointer.focusObject != null) {
                if (pointer.previousPosition.sqrMagnitude == 0)
                    pointer.data.delta = Vector2.zero;
                else
                    pointer.data.delta = pointer.data.position - pointer.previousPosition;
                pointer.previousPosition = pointer.data.position;

                pointer.touchPosition = pointer.data.pointerCurrentRaycast.worldPosition;
            }
        }

        private void CastPhysicsRayFromPointer(InteractionPointer pointer) {
            RaycastResult raycastResult = new RaycastResult();

            if (pointer.focusingEnabled && pointer.type == PointerType.Point) {
                if (pointer.externalRayCast) {
                    raycastResult.worldPosition = pointer.focusPosition;
                    raycastResult.worldNormal = pointer.focusRotation * Vector3.forward;
                    raycastResult.distance = (raycastResult.worldPosition - pointer.pointerTransform.position).magnitude;
                    raycastResult.gameObject = pointer.focusObject;
                }
                else {
                    Vector3 pointingDirection = pointer.pointerTransform.rotation * pointer.localPointingDirection;

                    RaycastHit hit;
                    bool raycastHit = Physics.Raycast(pointer.pointerTransform.position, pointingDirection * 10, out hit);
                    if (raycastHit) {
                        pointer.focusPosition = hit.point;
                        pointer.focusRotation = Quaternion.LookRotation(hit.normal);
                        pointer.focusObject = hit.transform.gameObject;

                        raycastResult.worldPosition = hit.point;
                        raycastResult.worldNormal = hit.normal;
                        raycastResult.distance = hit.distance;
                        raycastResult.gameObject = hit.transform.gameObject;
                    }
                    else {
                        pointer.focusPosition = pointer.pointerTransform.position + pointingDirection * 10;
                        pointer.focusRotation = Quaternion.LookRotation(-pointingDirection);
                        pointer.focusObject = null;

                        raycastResult.worldPosition = pointer.pointerTransform.position + pointingDirection * 10;
                        raycastResult.worldNormal = -pointingDirection;
                        raycastResult.distance = 0;
                        raycastResult.gameObject = null;
                    }

                    if (Camera.main != null) {
                        pointer.data.position = Camera.main.WorldToScreenPoint(pointer.focusPosition);
                        raycastResult.screenPosition = Camera.main.WorldToScreenPoint(pointer.focusPosition);
                    }
                }
            }
            else {
                pointer.focusPosition = pointer.pointerTransform.position;
                pointer.focusRotation = Quaternion.identity;
                pointer.focusObject = null;

                raycastResult.worldPosition = pointer.pointerTransform.position;
                raycastResult.worldNormal = Vector3.up;
                raycastResult.distance = 0;
                raycastResult.gameObject = null;

                if (Camera.main != null) {
                    pointer.data.position = Camera.main.WorldToScreenPoint(pointer.pointerTransform.position);
                    raycastResult.screenPosition = Camera.main.WorldToScreenPoint(pointer.focusPosition);
                }
            }

            pointer.data.pointerCurrentRaycast = raycastResult;
        }

        private void CastUIRayFromPointer(InteractionPointer pointer) {
            if (Camera.main == null)
                return;
            Debug.DrawLine(Camera.main.transform.position, pointer.focusPosition);
            Vector3 ray = Camera.main.ScreenToWorldPoint(pointer.data.position);
            Debug.DrawLine(Camera.main.transform.position, ray, Color.green);
            eventSystem.RaycastAll(pointer.data, m_RaycastResultCache);

            //pointer.data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            RaycastResult raycastResult = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();

            if (raycastResult.gameObject != null) {
                if (pointer.type == PointerType.Touch) {
                    float distance = DistanceTipToTransform(pointer.pointerTransform, raycastResult.gameObject.transform);
                    // Focus only when hovering
                    if (distance > 0.05F)
                        return;
                }

                pointer.data.pointerCurrentRaycast = raycastResult;
                pointer.focusObject = pointer.data.pointerCurrentRaycast.gameObject;
                // EventSystem.RaycastAll always casts from main.camera. This is why we need a trick to look like it is casting from the pointerlocation (e.g. finger)
                // The result does not look right in scene view, but does look OK in game view
                Vector3 focusDirection = (pointer.focusPosition - Camera.main.transform.position).normalized;
                pointer.focusPosition = Camera.main.transform.position + focusDirection * pointer.data.pointerCurrentRaycast.distance; // pointer.data.pointerCurrentRaycast.worldPosition == Vector.zero unfortunately
                                                                                                                                       //pointer.data.position = pointer.focusPosition;
                pointer.data.position = Camera.main.WorldToScreenPoint(pointer.focusPosition);

            }
        }

        private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords) {
            cartCoords.Normalize();
            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;
            float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
            if (cartCoords.x < 0)
                outPolar += Mathf.PI;
            float outElevation = Mathf.Asin(cartCoords.y);
            return new Vector2(outPolar, outElevation);
        }

        private float DistanceTipToTransform(Transform fingerTip, Transform transform) {
            Debug.DrawLine(fingerTip.position, transform.position);
            return (-transform.InverseTransformPoint(fingerTip.position).z * transform.lossyScale.z) - 0.01F;
        }
    }
}