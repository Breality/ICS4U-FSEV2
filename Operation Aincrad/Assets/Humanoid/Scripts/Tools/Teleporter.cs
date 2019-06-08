using UnityEngine;

namespace Passer {

    public class Teleporter : InteractionPointer {
        public enum TransportType {
            Teleport,
            Dash
        }
        public TransportType transportType = TransportType.Teleport;
        public Transform transformToTeleport;

        public void TeleportHumanoid() {
            HumanoidControl humanoid = transformToTeleport.GetComponent<HumanoidControl>();
            if (humanoid == null)
                transformToTeleport.Teleport(focusPointObj.transform.position);
            else {
                Vector3 interactionPointerPosition = humanoid.GetHumanoidPosition() - transformToTeleport.position;

                switch (transportType) {
                    case TransportType.Teleport:
                        transformToTeleport.Teleport(focusPointObj.transform.position - interactionPointerPosition);
                        break;
                    case TransportType.Dash:
                        StartCoroutine(TransformMovements.DashCoroutine(transformToTeleport, focusPointObj.transform.position - interactionPointerPosition));
                        break;
                    default:
                        break;
                }
            }
        }

        public static new Teleporter Add(Transform parentTransform, PointerType pointerType = PointerType.Ray) {
            GameObject pointerObj = new GameObject("Teleporter");
            pointerObj.transform.SetParent(parentTransform, false);

            GameObject destinationObj = new GameObject("Destination");
            destinationObj.transform.SetParent(pointerObj.transform);
            destinationObj.transform.localPosition = Vector3.zero;
            destinationObj.transform.localRotation = Quaternion.identity;

            if (pointerType == PointerType.FocusPoint) {
                GameObject focusPointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                focusPointSphere.transform.SetParent(destinationObj.transform);
                focusPointSphere.transform.localPosition = Vector3.zero;
                focusPointSphere.transform.localRotation = Quaternion.identity;
                focusPointSphere.transform.localScale = Vector3.one * 0.1F;
            }
            else {
                LineRenderer pointerRay = destinationObj.AddComponent<LineRenderer>();
                pointerRay.startWidth = 0.01F;
                pointerRay.endWidth = 0.01F;
                pointerRay.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                pointerRay.receiveShadows = false;
                pointerRay.useWorldSpace = false;
            }

            Teleporter teleporter = pointerObj.AddComponent<Teleporter>();
            teleporter.focusPointObj = destinationObj;
            teleporter.rayType = RayType.Bezier;

            return teleporter;
        }

    }
}
