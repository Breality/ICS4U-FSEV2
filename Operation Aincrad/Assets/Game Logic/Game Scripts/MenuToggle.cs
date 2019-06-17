using System.Collections;
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
 * Menu Toggle Class
 * Description: A class that allows the user to navigate the UI menu.
 */


public class MenuToggle : MonoBehaviour
{
    // ------------- menu toggling -------------
    public GameObject MenuOptions;
    public Transform display;
    public Transform extensions;
    public GameObject option;
    public GameObject Camera;

    //When the buttons are animating
    bool inTranslation = false;

    //If an option is open
    bool isOpen = false;

    //For the current menu option the user is on
    private int cur = 0;
    private int curSecondaryMenu = -1;
    float transitionConstant = 0.09f;

    private List<GameObject> ButtonParents = new List<GameObject> { }; // buttons we care about depending on what is being displayed, parents are used to allow dynamic changes to allowed buttons

    //Menu Buttons
    private string[] order = new string[] { "Start", "Profile", "Friends", "Map", "Settings" };
    private string[][] buttons = new string[][] {
        new string[] { },
        new string[] { "Stats", "Equipment", "Items", "Titles" }, 
        new string[] { "Create Party", "Join Party" , "Friends", "Messages"}, 
        new string[] { "Maps"}, 
        new string[] { "Music"}, 
    };

    //To point at each separate scripts
    private Dictionary<string, Equipment> ScriptReferences = new Dictionary<string, Equipment> { };

    
    //Either shows the correct menu options or removes them
    private void ButtonMode(Transform button, bool mode)
    {
        if (mode)
        {
            //Change colour to show its selected
            button.GetComponent<Image>().color = new Color32(0, 245, 255, 137);
            for (int i = 0; i < buttons[cur].Length; i++) // make all the texts
            {
                try
                {
                    GameObject newOption = Instantiate(option);
                    newOption.transform.SetParent(extensions);

                    newOption.transform.localPosition = new Vector3(option.transform.localPosition.x, -0.05f * i, option.transform.localPosition.z);
                    newOption.transform.rotation = option.transform.rotation;

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
        //Remove the options
        else
        {
            //Change colour of button back to unselected
            button.GetComponent<Image>().color = new Color32(255, 255, 255, 121);
            foreach (Transform child in extensions) { Destroy(child.gameObject); }

            if (CurrentDisplay != null) // there was a display, turn that off
            {
                display.Find(CurrentDisplay).gameObject.SetActive(false); 
                CurrentDisplay = null;
            }
        }
    }

    //Animates the buttons moving - it's on a separate thread to not stall the main loop
    public IEnumerator Toggle(string menuName) 
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

            // Translate buttons
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime + transitionTime)
            {
                // This changes the object's local position based on the camera
                MenuOptions.transform.localPosition = new Vector3(MenuOptions.transform.localPosition.x,
                    startPos + translation * (Time.realtimeSinceStartup - startTime) / transitionTime - startingoffset,
                    MenuOptions.transform.localPosition.z);
                yield return new WaitForSeconds(0.001f);
            }
            MenuOptions.transform.localPosition = new Vector3(MenuOptions.transform.localPosition.x, startPos + translation - startingoffset, MenuOptions.transform.localPosition.z);

            ButtonMode(MenuOptions.transform.Find(newName), true); // Show new menu
            inTranslation = false; // Allow translation again after animation has finished
        }
        yield return null;

    }

    //Activates the UI (it's initially hidden)
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
    
    // ------------- Connecting the menu and the raycasting -------------
    //When option isn't selected
    private void UnHover(GameObject item)
    {
        Transform par = item.transform.parent;

        if (item.transform.IsChildOf(MenuOptions.transform)) // The 4 menu options
        {
            Image image = item.GetComponent<Image>();
            image.color = new Color32(Convert.ToByte(image.color.r*255), Convert.ToByte(image.color.g*255), Convert.ToByte(image.color.b*255), 121);
        }else if (item.transform.IsChildOf(extensions)) // The buttons we create
        {
            item.transform.Find("Panel").GetComponent<Image>().color = new Color32(0, 0, 0, 111);
        }else if (par != null && ButtonParents.Contains(par.gameObject)) 
        {
            ScriptReferences[CurrentDisplay].UnHover(item);
        }
    }

    //Shows UI response when ray is hovered over it
    private void Hover(GameObject item)
    {
        Debug.Log("Hover");
        if (item.transform.IsChildOf(MenuOptions.transform)) // the 4 menus
        {
            Image image = item.GetComponent<Image>();
            image.color = new Color32(Convert.ToByte(image.color.r * 255), Convert.ToByte(image.color.g * 255), Convert.ToByte(image.color.b * 255), 30);
        }else if (item.transform.IsChildOf(extensions)) // the buttons we create
        {
            item.transform.Find("Panel").GetComponent<Image>().color = new Color32(0, 255, 236, 111);
        }else if (ButtonParents.Contains(item.transform.parent.gameObject)) 
        {
            Debug.Log("Letting them know of hover");
            ScriptReferences[CurrentDisplay].Hover(item);
        }

    }

    string CurrentDisplay = null;

    //Decideds what happens when the option is clicked
    private void Click(GameObject item)
    {
        Debug.Log("Clicked on " + item.name);
        if (item.transform.IsChildOf(MenuOptions.transform)) // 4 Menu options
        {
            StartCoroutine(Toggle(item.name));
            
        }
        else if (item.transform.IsChildOf(extensions))
        {
            CurrentDisplay = item.name;
            display.Find(CurrentDisplay).gameObject.SetActive(true);
            ButtonParents = ScriptReferences[CurrentDisplay].Activated();
            foreach (Transform child in extensions) { Destroy(child.gameObject); } // remove the options and display what is wanted
            Debug.Log(ButtonParents.Count);
        }
        else if (ButtonParents.Contains(item.transform.parent.gameObject)) 
        {
            Debug.Log("Calling their clicked function");
            ScriptReferences[CurrentDisplay].Clicked(item);
        }
    }

    float lastswitch = 0;
    public void SwitchActive() // Detects if they open a menu and starts raycasting
    {
        if (Time.realtimeSinceStartup - lastswitch > 1)
        {
            Activate(!isOpen);
            lastswitch = Time.realtimeSinceStartup;
        }
    }

    // ------------- ray casting------------- for when you select an option with the controller
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
        ScriptReferences = new Dictionary<string, Equipment> {
            { "Equipment", new Equipment(Camera) }
        };

        //Created the right and left line renderers
        rightLine = rightL.transform.GetComponent<LineRenderer>();
        leftLine = leftL.transform.GetComponent<LineRenderer>();
        Debug.Log("Started with " + rightLine + " and " + leftLine);
        initLine(rightLine);
        initLine(leftLine);
        Debug.Log("Stuff started");
    }

    //Decides how the lines will be drawn
    void initLine(LineRenderer line)
    {
        Vector3[] initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        line.SetPositions(initLaserPositions);
        line.startWidth = line.endWidth = lineThickness;
        line.material.color = Color.cyan;
    }

    //This draws the ray onto the screen
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
                //Right Controller
                if (interactState.source.handedness == InteractionSourceHandedness.Right)
                {
                    Debug.DrawRay(lHand.position, sourceGripRot);

                    DrawRay(rHand.position, sourceGripRot, interactState.source.handedness);

                }
                //Left Controller
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
        //Simply draws the ray onto the screen
        DrawRay();
        if (lHover != null) { UnHover(lHover); } // remove hover at start of frame and refind what is being hovered on 
        if (rHover != null) { UnHover(rHover); }

        //Gets all of the colliders the ray hit
        RaycastHit[] collided = GetColliders("right");
        //Checks if the right controller hit the button
        rHover = CheckCollided(collided);
        collided = GetColliders("left");
        //Checks if the left controller hit the button
        lHover = CheckCollided(collided);

        //If a controller button gets pressed, check if the raycast hit one of the buttons
        if (Input.GetButton("L_Trigger") && lHover != null)
        {
            Click(lHover);
        }
        else if (Input.GetButton("R_Trigger") && rHover != null)
        {
            Click(rHover);
        }
        
    }

    //Detects if the ray hit anything (colliders on the buttons)
    private void DrawRay(Vector3 pos, Vector3 forw, InteractionSourceHandedness handedness)
    {
        //Right controller
        if (handedness == InteractionSourceHandedness.Right)
        {
            rightLine.SetPosition(0, pos);
            rightLine.SetPosition(1, pos + forw * rayLen);
            rightLine.enabled = true;
            Ray ray = new Ray(pos, forw);
            rHandCol = Physics.RaycastAll(ray, Mathf.Infinity);
        }
        //Left controller
        if (handedness == InteractionSourceHandedness.Left)
        {
            Debug.Log(leftLine);
            leftLine.SetPosition(0, pos);
            leftLine.SetPosition(1, pos + forw * rayLen);
            leftLine.enabled = true;
            Ray ray = new Ray(pos, forw);
            lHandCol = Physics.RaycastAll(ray, Mathf.Infinity);
        }
    }


    //Returns all objects that are colliding with the ray
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

    //Checks if it collided with an actual button
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
