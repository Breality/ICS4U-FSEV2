using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RayCast2 : MonoBehaviour
{
    // Start is called before the first frame update
    public Image image;
    public TMP_Text cost;
    public Button purchase;
    public InfoCenter info;

    private string isDisplayed = null;
    private bool canBuy = true; // debounce for when they dont have enough money and buy again
    
    private void Display(Image orig, string name)
    {
        image.sprite = orig.sprite;
        cost.text = name + " $" + info.goldShop[name].ToString();
        isDisplayed = name;
    }
    

    private IEnumerator Buy()
    {
        if (isDisplayed != null && canBuy)
        {
            canBuy = false;
            bool success = info.gold >= info.goldShop[isDisplayed];
            if (success)
            {
                StartCoroutine(info.Request("Purchase", isDisplayed));
            }

            Color oldColor = purchase.GetComponent<Image>().color;
            purchase.GetComponent<Image>().color = success ? new Color(0, 255, 0) : new Color(255, 0, 0);
            purchase.GetComponentInChildren<TMP_Text>().text = success ? "Purchased" : "Insufficient Funds";

            yield return new WaitForSeconds(1.5f);
            purchase.GetComponentInChildren<TMP_Text>().text = "Purchase";
            purchase.GetComponent<Image>().color = oldColor;
            canBuy = true;
        }
    }

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
        //DrawRay(rHand.position, rHand.parent.position, "right");
        //DrawRay(lHand.position, lHand.parent.position, "left");
        var interactionSourceStates = InteractionManager.GetCurrentReading();
        foreach (var interactState in interactionSourceStates)
        {

            var sourcePose = interactState.sourcePose;
            Vector3 sourceGripRot;
            if (sourcePose.TryGetForward(out sourceGripRot, InteractionSourceNode.Pointer))
            {
                Debug.Log(interactState.source.handedness);
                if (interactState.source.handedness == InteractionSourceHandedness.Right)
                {
                    Debug.DrawRay(lHand.position, sourceGripRot);

                    DrawRay(rHand.position, sourceGripRot, interactState.source.handedness);

                }
                if (interactState.source.handedness == InteractionSourceHandedness.Left)
                {
                    Debug.DrawRay(lHand.position, sourceGripRot);
                    DrawRay(lHand.position, sourceGripRot, interactState.source.handedness);
                }
            }
        }
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
        if (Input.GetButton("L_Trigger") && lHover != null &&lHover.name.Equals("Purchase")) {
            Buy();
        }
        else if (Input.GetButton("R_Trigger") && rHover != null && rHover.name.Equals("Purchase"))
        {
            Buy();
        }
        else if (Input.GetButton("L_Trigger") && lHover != null )
        {
            EventSystem.current.SetSelectedGameObject(lHover);
            Display(lHover.GetComponent<Image>(), lHover.name);
        }
        else if (Input.GetButton("R_Trigger") && rHover != null)
        {
            EventSystem.current.SetSelectedGameObject(rHover);
            Display(rHover.GetComponent<Image>(), rHover.name);
        }
    }

    private void DrawRay(Vector3 pos, Vector3 forw, InteractionSourceHandedness handedness)
    {
        if (handedness == InteractionSourceHandedness.Right)
        {
            rightLine.SetPosition(0, pos);
            rightLine.SetPosition(1, pos+forw*rayLen);
            rightLine.enabled = true;
            Ray ray = new Ray(pos,forw);
            rHandCol = Physics.RaycastAll(ray, Mathf.Infinity);
        }
        if (handedness == InteractionSourceHandedness.Left)
        {
            leftLine.SetPosition(0, pos);
            leftLine.SetPosition(1, pos + forw * rayLen);
            leftLine.enabled = true;
            Ray ray = new Ray(pos, forw);
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
