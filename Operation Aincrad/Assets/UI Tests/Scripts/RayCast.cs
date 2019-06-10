using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
public class RayCast : MonoBehaviour
{
    // Start is called before the first frame update
    private float rayLen = 3.5f;
    private LineRenderer rightLine, leftLine;
    [SerializeField]
    private Transform rightL, leftL;
    [SerializeField]
    private float lineThickness;
    void Start()
    {
        rightLine = rightL.transform.GetComponent<LineRenderer>();
        leftLine = leftL.transform.GetComponent<LineRenderer>();
        initLine(rightLine);
        initLine(leftLine);

    }

    void initLine(LineRenderer line)
    {
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        line.SetPositions(initLaserPositions);
        line.startWidth = line.endWidth = lineThickness;
        line.material.color = Color.cyan;
    }

    public string[] rayCalc()
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
                //Debug.DrawRay(sourceGripPos, sourceGripRot);
                return DrawRay(sourceGripPos, sourceGripRot, interactState.source.handedness);
            }
        }
        return new string[] { "",""};
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rayCalc());
        
    }
    private string[] DrawRay(Vector3 pos, Vector3 forw, InteractionSourceHandedness handedness)
    {
        string[] collision = new string[] {"",""};
        if (handedness == InteractionSourceHandedness.Right)
        {
            rightLine.SetPosition(0, pos);
            rightLine.SetPosition(1, pos + forw * rayLen);
            rightLine.enabled = true;
            RaycastHit hit;
            if(Physics.Raycast(pos,forw,out hit))
            {
                collision[0] = (hit.transform.name);
            }
        }
        if (handedness == InteractionSourceHandedness.Left)
        {
            leftLine.SetPosition(0, pos);
            leftLine.SetPosition(1, pos + forw * rayLen);
            leftLine.enabled = true;
            RaycastHit hit;
            if (Physics.Raycast(pos, forw, out hit))
            {
                collision[1] = ( hit.transform.name);
            }
        }
        return collision;
    }


}