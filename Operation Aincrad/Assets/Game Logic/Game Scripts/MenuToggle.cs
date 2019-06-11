using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class MenuToggle : MonoBehaviour
{
    // ------------- menu toggling -------------
    public GameObject MenuOptions;
    public Transform display;
    public Transform extensions;
    public GameObject option;

    bool inTranslation = false;
    bool isOpen = false;
    private int cur = 0;

    private List<GameObject> ButtonParents = new List<GameObject> { }; // buttons we care about depending on what is being displayed, parents are used to allow dynamic changes to allowed buttons
    
    private string[] order = new string[] { "Start", "Profile", "Friends", "Map", "Settings" };
    private string[][] buttons = new string[][] {
        new string[] { },
        new string[] { "Stats", "Equipment", "Items", "Titles" }, // finish all these menus today (Tuesday)
        new string[] { "Create Party", "Join Party" , "Friends", "Messages"}, // finish all these menus after battle basics are done
        new string[] { "Maps"}, // later
        new string[] { "Music"}, // later
    };
    
    
    private int curSecondaryMenu = -1;
    float transitionConstant = 0.09f;

    private void ButtonMode(Transform button, bool mode)
    {
        if (mode)
        {
            button.GetComponent<Image>().color = new Color32(0, 245, 255, 137);
            for (int i = 0; i < buttons[cur].Length; i++) // make all the texts
            {
                try
                {
                    GameObject newOption = Instantiate(option);
                    newOption.transform.SetParent(extensions);

                    newOption.transform.localPosition = new Vector3(newOption.transform.localPosition.x, -0.05f * i, newOption.transform.localPosition.z);
                    newOption.name = buttons[cur][i];
                    newOption.transform.Find("Text").GetComponent<TMP_Text>().text = buttons[cur][i];

                    newOption.SetActive(true);
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }

            }
        }
        else
        {
            button.GetComponent<Image>().color = new Color32(255, 255, 255, 121);
            extensions.DetachChildren();
            if (CurrentDisplay != null) // there was a display, turn that off
            {
                display.Find(CurrentDisplay).gameObject.SetActive(false); 
                CurrentDisplay = null;
            }
        }
    }




    public IEnumerator Toggle(string menuName) // scrolls through the menu options
    {
        int dir = Array.IndexOf(order, menuName) - cur; 
        float transitionTime = 0.5f * Mathf.Abs(dir);
        if (!inTranslation && cur + dir > 0 && cur + dir < order.Length) // only toggle if we're not already toggling and if we have room
        {
            inTranslation = true;

            // get the variables
            string oldName = order[cur];
            cur += dir;
            string newName = order[cur];
            float startPos = MenuOptions.transform.localPosition.y;
            float translation = dir * transitionConstant;
            float startingoffset = 0;

            ButtonMode(MenuOptions.transform.Find(oldName), false); // unmark button and remove old menu

            // translate buttons
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime + transitionTime)
            {
                // this changes the object's position in world space, should be based on camera
                MenuOptions.transform.localPosition = new Vector3(MenuOptions.transform.localPosition.x,
                    startPos + translation * (Time.realtimeSinceStartup - startTime) / transitionTime - startingoffset,
                    MenuOptions.transform.localPosition.z);
                yield return new WaitForSeconds(0.001f);
            }
            MenuOptions.transform.localPosition = new Vector3(MenuOptions.transform.localPosition.x, startPos + translation - startingoffset, MenuOptions.transform.localPosition.z);

            ButtonMode(MenuOptions.transform.Find(newName), true); // show new menu
            inTranslation = false; // allow translation again
        }
        yield return null;

    }

    public void Activate(bool value)
    {
        if (value && cur > 0) // reset the values
        {
            ButtonMode(MenuOptions.transform.Find(order[cur]), false);
            MenuOptions.transform.Translate(new Vector3(0, transitionConstant * cur * -1, 0));
            cur = 0;
        }
        MenuOptions.transform.parent.gameObject.SetActive(value);
        isOpen = value;
    }
    
    // ------------- connecting the menu and the raycasting -------------
    private void UnHover(GameObject item)
    {
        if (item.transform.IsChildOf(MenuOptions.transform)) // the 4 menus
        {
            Image image = item.GetComponent<Image>();
            image.color = new Color32(Convert.ToByte(image.color.r*255), Convert.ToByte(image.color.g*255), Convert.ToByte(image.color.b*255), 121);
        }else if (item.transform.IsChildOf(extensions)) // the buttons we create
        {
            item.transform.Find("Panel").GetComponent<Image>().color = new Color32(0, 0, 0, 111);
        }else if (ButtonParents.Contains(item.transform.parent.gameObject)) // specific display script should know about this
        {

        }
    }

    private void Hover(GameObject item)
    {
        if (item.transform.IsChildOf(MenuOptions.transform)) // the 4 menus
        {
            Image image = item.GetComponent<Image>();
            image.color = new Color32(Convert.ToByte(image.color.r * 255), Convert.ToByte(image.color.g * 255), Convert.ToByte(image.color.b * 255), 30);
        }else if (item.transform.IsChildOf(extensions)) // the buttons we create
        {
            item.transform.Find("Panel").GetComponent<Image>().color = new Color32(0, 255, 236, 111);
        }else if (ButtonParents.Contains(item.transform.parent.gameObject)) // specific display script should know about this
        {

        }

    }

    string CurrentDisplay = null;
    private void Click(GameObject item)
    {
        Debug.Log("Clicked on " + item.name);
        if (item.transform.IsChildOf(MenuOptions.transform)) // the 4 menus
        {
            StartCoroutine(Toggle(item.name));
        }
        else if (item.transform.IsChildOf(extensions))
        {
            CurrentDisplay = item.name;
            display.Find(item.name).gameObject.SetActive(true);
            extensions.DetachChildren(); // remove the options and display what is wanted
        }else if (ButtonParents.Contains(item.transform.parent.gameObject)) // specific display script should know about this
        { 

        }
    }

    float lastswitch = 0;
    public void SwitchActive() // this function should actually be in another one just detecting when they open the menu, which starts the raycasts too
    {
        if (Time.realtimeSinceStartup - lastswitch > 1)
        {
            Activate(!isOpen);
            lastswitch = Time.realtimeSinceStartup;
        }
    }

    // ------------- ray casting-------------
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


        // draw any affects for selected items right over here
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
