﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Shop Toggle Script
 * Description: Deals with the shop UI among the list of commands in the UI
 */

public class ShopToggle : MonoBehaviour
{
    // ------------- menu toggling -------------
    public HTTPClient HTTP;
    public Image image;
    public TMP_Text cost;
    public Button purchase;
    public InfoCenter info;
    public Transform SaleOptions;

    private string isDisplayed = null;
    private bool canBuy = true; // debounce for when they dont have enough money and buy again
    private GameObject selected = null;

    private void Display(Image orig, string name)
    {
        image.sprite = orig.sprite;
        cost.text = name + " $" + info.goldShop[name].ToString();
        isDisplayed = name;
    }

    //Checks logic to see if buying is possible
    private IEnumerator Buy()
    {
        if (isDisplayed != null && canBuy)
        {
            canBuy = false;
            bool success = info.gold >= info.goldShop[isDisplayed];
            if (success)
            {
                HTTP.AskServer(new Dictionary<string, string> { { "request" , "Purchase"}, {"Item name", isDisplayed } });
            }

            purchase.GetComponent<Image>().color = success ? new Color(0, 255, 0) : new Color(255, 0, 0); // will be for a milisecond, then go back to onhover
            purchase.GetComponentInChildren<TMP_Text>().text = success ? "Purchased" : "Insufficient Funds";

            yield return new WaitForSeconds(1.5f);
            purchase.GetComponentInChildren<TMP_Text>().text = "Purchase";
            canBuy = true;
        }
    }

    // ------------- connecting the menu and the raycasting -------------
    private void UnHover(GameObject item)
    {
        if (item.name.Equals("Purchase"))
        {
            item.GetComponent<Image>().color = new Color32(255, 255, 255, 193);
        }
        else if (item.transform.IsChildOf(SaleOptions)) // purchase options
        {
            item.GetComponent<Image>().color = new Color32(255, 255, 255, 115);
            item.transform.Find("Panel ").GetComponent<Image>().color = new Color32(147, 142, 142, 155);
        }
    }

    //Once the raycast is over the item, show to the user that it's being hovered over by changing colours of the buttons and panels
    private void Hover(GameObject item)
    {
        if (item.name.Equals("Purchase"))
        {
            item.GetComponent<Image>().color = new Color32(0, 226, 255, 128);
        }
        else if (item.transform.IsChildOf(SaleOptions)) // purchase options
        {
            item.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            item.transform.Find("Panel ").GetComponent<Image>().color = new Color32(147, 142, 142, 21);
        }

    }
    

    //Detect selection of the menu
    private void Click(GameObject item)
    {
        Debug.Log("Clicked on " + item.name);
        if (item.name.Equals("Purchase"))
        {
            StartCoroutine(Buy());
        }
        else if (item.transform.IsChildOf(SaleOptions)) // purchase options
        {
            EventSystem.current.SetSelectedGameObject(item);
            Display(item.GetComponent<Image>(), item.name);

            if (selected != null) { UnHover(selected); }

            selected = item;
        }

    }


    // ------------- ray casting------------- similar to MenuToggle
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

    public void DrawRay()
    {
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

    void Update()
    {
        DrawRay();
        if (lHover != null) { UnHover(lHover); } // remove hover at start of frame and refind what is being hovered on 
        if (rHover != null) { UnHover(rHover); }

        RaycastHit[] collided = GetColliders("right");
        rHover = CheckCollided(collided);
        collided = GetColliders("left");
        lHover = CheckCollided(collided);

        if (Input.GetButton("L_Trigger") && lHover != null)
        {
            Click(lHover);
        }
        else if (Input.GetButton("R_Trigger") && rHover != null)
        {
            Click(rHover);
        }

    }

    private void DrawRay(Vector3 pos, Vector3 forw, InteractionSourceHandedness handedness)
    {
        if (handedness == InteractionSourceHandedness.Right)
        {
            rightLine.SetPosition(0, pos);
            rightLine.SetPosition(1, pos + forw * rayLen);
            rightLine.enabled = true;
            Ray ray = new Ray(pos, forw);
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
        if (hand == "right")
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
            if (collide.collider.tag == "Button")
            {
                GameObject item = collide.collider.gameObject;
                Hover(item);
                return item;
            }
        }
        return null;
    }


}
