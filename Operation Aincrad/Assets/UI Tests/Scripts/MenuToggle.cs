using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuToggle : MonoBehaviour
{
    //For the displaying things
    //public Scroll show;
    public Transform display;
    [SerializeField]
    private RayCast ray;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
    }



    private void ButtonMode(Transform button, bool mode)
    {
        if (mode)
        {
            button.GetComponent<Image>().color = new Color32(255, 181, 0, 255);
            extensions.Find(button.name).gameObject.SetActive(true);
        }
        else
        {
            button.GetComponent<Image>().color = new Color32(255, 255, 225, 121);
            extensions.Find(button.name).gameObject.SetActive(false);
        }
    }

    bool inTranslation = false;
    bool isOpen = false;
    private int cur = 0;
    public Transform options;
    public Transform extensions;
    private string[] order = new string[] { "Start", "Profile", "Friends", "Map", "Settings" };
    private List<string> button = new List<string> { "Inventory", "Stats", "Equipment", "Chat", "View Friends", "Send Request", "Map", "Sound", "Sensitivity", "Voice Chat" };

    float transitionConstant = 0.09f;

    public IEnumerator Toggle(int dir) // scrolls through the menu options
    {
        float transitionTime = 0.5f * Mathf.Abs(dir);
        Debug.Log("Hello");
        if (!inTranslation && cur + dir > 0 && cur + dir < order.Length) // only toggle if we're not already toggling and if we have room
        {
            inTranslation = true;

            // get the variables
            string oldName = order[cur];
            cur += dir;
            string newName = order[cur];
            float startPos = options.localPosition.y;
            float translation = dir * transitionConstant;
            float startingoffset = 0;
                //options.parent.position.y;

            ButtonMode(options.Find(oldName), false); // unmark button and remove old menu

            // translate buttons
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime + transitionTime)
            {
                // this changes the object's position in world space, should be based on camera
                options.localPosition = new Vector3(options.localPosition.x,
                    startPos + translation * (Time.realtimeSinceStartup - startTime) / transitionTime  - startingoffset,
                    options.localPosition.z);
                yield return new WaitForSeconds(0.001f);
            }
            options.localPosition = new Vector3(options.localPosition.x, startPos + translation - startingoffset, options.localPosition.z);

            ButtonMode(options.Find(newName), true); // show new menu
            inTranslation = false; // allow translation again
        }
        yield return null;

    }
    // Update is called once per frame
    public void Activate(bool value)
    {
        print(value);
        if (value && cur > 0) // reset the values
        {
            ButtonMode(options.Find(order[cur]), false);
            options.Translate(new Vector3(0, transitionConstant * cur *-1, 0));
            cur = 0;
        }
        options.parent.gameObject.SetActive(value);
        isOpen = value;
    }

    float lastswitch = 0;
    public void switchSeen() // for testing
    {
        if (Time.realtimeSinceStartup - lastswitch > 1)
        {
            Activate(!isOpen);
            lastswitch = Time.realtimeSinceStartup;
        }
    }

    public void clicked(string button)
    {
        /*
        if (button == "Inventory")
        {
            show.Begin(button, inventory, "Grid");
            Debug.Log("Inventory clicked.");

            //Show inventory - got to reference the null object containing swords

        }
        else
        {
            Debug.Log("nope");
        }
        */

        Debug.Log(display.transform.Find(button).transform.GetComponent(button));
        /*
        foreach(Transform option in display)
        {
            Debug.Log(option.name);
            Debug.Log(option.GetComponents(typeof(Component)));
        }*/

    }


    void Update()
    {
        if (Input.GetKey(KeyCode.K)) // up
        {
            Debug.Log("K value being presed");
            StartCoroutine(Toggle(-1));
        }
        if (Input.GetKey(KeyCode.M)) // down
        {
            StartCoroutine(Toggle(1));
        }

        if (Input.GetButton("R_Trigger"))
        {
            RaycastHit[] collided = ray.GetColliders("Right");
        }
        ray.rayCalc();
        if (Input.GetButton("L_Trigger"))
        {
            RaycastHit[] collided = ray.GetColliders("Left");
        }        
        //Debug.Log("righy" + collision[0]);
        //Debug.Log("left" + collision[1]);
        
    }
}
