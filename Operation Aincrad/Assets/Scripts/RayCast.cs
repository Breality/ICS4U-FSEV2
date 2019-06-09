using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
public class RayCast : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
    {
        var interactionSourceState = args.state;
        var sourcePose = interactionSourceState.sourcePose;
        Vector3 sourceGripPosition;
        Quaternion sourceGripRotation;
        if ((sourcePose.TryGetPosition(out sourceGripPosition, InteractionSourceNode.Pointer)) &&
            (sourcePose.TryGetRotation(out sourceGripRotation, InteractionSourceNode.Pointer)))
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(sourceGripPosition, sourceGripRotation * Vector3.forward, out raycastHit, 10))
            {
                var targetObject = raycastHit.collider.gameObject;
                // ...
            }
        }
    }
}
