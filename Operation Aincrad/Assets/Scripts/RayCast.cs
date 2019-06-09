using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
public class RayCast : MonoBehaviour
{
    // Start is called before the first frame update
    private float rayLen = 1.5f;
    private LineRenderer rightLine;
    void Start()
    {
        rightLine = this.transform.GetComponent<LineRenderer>();
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        rightLine.SetPositions(initLaserPositions);
        rightLine.endWidth = 0;
        rightLine.startWidth = 0.1f;
        rightLine.startColor = Color.black;
    }

    // Update is called once per frame
    void Update()
    {

        var interactionSourceStates = InteractionManager.GetCurrentReading();
        foreach (var interactState in interactionSourceStates)
        {

            Debug.Log(interactState.source.handedness);
            var sourcePose = interactState.sourcePose;
            Vector3 sourceGripPos;
            Vector3 sourceGripRot;
            if (sourcePose.TryGetPosition(out sourceGripPos, InteractionSourceNode.Pointer) && sourcePose.TryGetForward(out sourceGripRot, InteractionSourceNode.Pointer))
            {
                Debug.DrawRay(sourceGripPos, sourceGripRot);
                DrawRay(sourceGripPos, sourceGripRot, interactState.source.handedness);
            }
        }
    }
    private void DrawRay(Vector3 pos, Vector3 forw, InteractionSourceHandedness handedness)
    {
        if (handedness == InteractionSourceHandedness.Right)
        {
            rightLine.SetPosition(0, pos);
            rightLine.SetPosition(1, pos + forw * rayLen);
            rightLine.enabled = true;
        }
        




    }

}