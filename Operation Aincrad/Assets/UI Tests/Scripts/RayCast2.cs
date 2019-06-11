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
    private GameObject selected = null;
    
    private void Display(Image orig, string name)
    {
        image.sprite = orig.sprite;
        cost.text = name + " $" + info.goldShop[name].ToString();
        isDisplayed = name;
    }
    

    private IEnumerator Buy()
    {
        Debug.Log("Fun purchase time");
        if (isDisplayed != null && canBuy)
        {
            canBuy = false;
            bool success = info.gold >= info.goldShop[isDisplayed];
            if (success)
            {
                StartCoroutine(info.Request("Purchase", isDisplayed));
            }
            
            purchase.GetComponent<Image>().color = success ? new Color(0, 255, 0) : new Color(255, 0, 0); // will be for a milisecond, then go back to onhover
            purchase.GetComponentInChildren<TMP_Text>().text = success ? "Purchased" : "Insufficient Funds";

            yield return new WaitForSeconds(1.5f);
            purchase.GetComponentInChildren<TMP_Text>().text = "Purchase";
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
    void unHover(GameObject item)
    {
        // sword transparency
        if (item.name.Equals("Purchase")){
            item.GetComponent<Image>().color = new Color32(255, 255, 255, 193);
        }
        else
        {
            item.GetComponent<Image>().color = new Color32(255, 255, 255, 115);
            item.transform.Find("Panel ").GetComponent<Image>().color = new Color32(147, 142, 142, 155);
        }
       
    }


    void Update()
    {
        rayCalc();
        if (lHover != null) { unHover(lHover); }
        if (rHover != null) { unHover(rHover); }

        RaycastHit[] collided = GetColliders("right");
        rHover = CheckCollided(collided);
        collided = GetColliders("left");
        lHover = CheckCollided(collided);

        if (Input.GetButton("L_Trigger") && lHover != null && lHover.name.Equals("Purchase")) {
            StartCoroutine(Buy());
        }
        else if (Input.GetButton("R_Trigger") && rHover != null && rHover.name.Equals("Purchase"))
        {
            StartCoroutine(Buy());
        }
        else if (Input.GetButton("L_Trigger") && lHover != null )
        {
            EventSystem.current.SetSelectedGameObject(lHover);
            Display(lHover.GetComponent<Image>(), lHover.name);

            if (selected != null) { unHover(selected); }

            selected = lHover;
        }
        else if (Input.GetButton("R_Trigger") && rHover != null)
        {
            Debug.Log(rHover.name);
            Debug.Log(rHover.GetComponent<Image>()); ;
            Display(rHover.GetComponent<Image>(), rHover.name);

            if (selected != null) { unHover(selected); }

            selected = rHover;
        }


        if (selected != null)
        {
            selected.transform.Find("Panel ").GetComponent<Image>().color = new Color32(0, 255, 255, 100);
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
            if(collide.collider.tag == "Button")
            {
                GameObject item = collide.collider.gameObject;
                if (item.name.Equals("Purchase"))
                {
                    item.GetComponent<Image>().color = new Color32(0, 226, 255, 128);
                }
                else
                {
                    item.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    item.transform.Find("Panel ").GetComponent<Image>().color = new Color32(147, 142, 142, 21);
                }
                
                

                return item;
            }
        }
        return null;
    }


}
