using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RayCast : MonoBehaviour
{
    // Start is called before the first frame update
    private float rayLen = 5f;
    private LineRenderer rightLine, leftLine;
    [SerializeField]
    private Transform rightL, leftL;
    [SerializeField]
    private float lineThickness;
    [SerializeField]
    private Transform lHand, rHand;
    GameObject lHover = null, rHover = null;
    private RaycastHit[] rHandCol, lHandCol;
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

    public void rayCalc()
    {
        DrawRay(rHand.position, rHand.parent.position, "right");
        DrawRay(lHand.position, lHand.parent.position, "left");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rayCalc());
        rayCalc();
        if (lHover != null)
        {
            Color c = lHover.GetComponent<Image>().color;
            c.a = 1f;
            lHover.GetComponent<Image>().color = c;
        }
        if (rHover != null)
        {
            Color c = rHover.GetComponent<Image>().color;
            c.a = 1f;
            rHover.GetComponent<Image>().color = c;

        }
        RaycastHit[] collided = GetColliders("right");
        rHover = CheckCollided(collided);
        collided = GetColliders("left");
        lHover = CheckCollided(collided);
        Debug.Log(Input.GetButton("R_Trigger"));
        Debug.Log(rHover);
        if (Input.GetButton("L_Trigger") && lHover != null)
        {
            EventSystem.current.SetSelectedGameObject(lHover);
            Debug.Log(lHover.name + " SELECTED");
        }
        else if (Input.GetButton("R_Trigger") && rHover != null)
        {
            EventSystem.current.SetSelectedGameObject(rHover);
            Debug.Log(rHover.name + " SELECTED");
        }

    }
    private void DrawRay(Vector3 toPos, Vector3 fromPos, string handedness)
    {
        if (handedness == "right")
        {
            rightLine.SetPosition(0, toPos);
            rightLine.SetPosition(1, (toPos-fromPos)*rayLen+toPos);
            rightLine.enabled = true;
            Ray ray = new Ray(fromPos, toPos - fromPos);
            rHandCol = Physics.RaycastAll(ray, Mathf.Infinity);
        }
        if (handedness == "left")
        {
            leftLine.SetPosition(0, toPos);
            leftLine.SetPosition(1, (toPos - fromPos) * rayLen + toPos);
            leftLine.enabled = true;
            Ray ray = new Ray(fromPos, toPos-fromPos);
            lHandCol = Physics.RaycastAll(ray, Mathf.Infinity);
        }
    }
    public RaycastHit[] GetColliders(string hand)
    {
        if(hand == "right")
        {
            return rHandCol;
        }
        else
        {
            return lHandCol;
        }
    }
    GameObject CheckCollided(RaycastHit[] collisions)
    {
        foreach (RaycastHit collide in collisions)
        {
            Debug.Log(collide.collider.tag);
            if(collide.collider.tag == "Button")
            {
                Color c = collide.collider.GetComponent<Image>().color;
                c.a = 0.5f;
                collide.collider.GetComponent<Image>().color = c;
                return collide.collider.gameObject;
            }
            
        }
        return null;
    }

}