using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuToggle : MonoBehaviour
{
    //For the displaying things
    //public Scroll show;
    public Transform display;
    [SerializeField]
    //private RayCast ray;
    // Start is called before the first frame update
    
    bool inTranslation = false;
    bool isOpen = false;
    private int cur = 0;
    public Transform choices;
    public Transform extensions;
    public GameObject option;

    private string[] order = new string[] { "Start", "Profile", "Friends", "Map", "Settings" };
    private string[][] buttons = new string[][] {
        new string[] { },
        new string[] { "Inventory", "Stats", "Equip" },
        new string[] { "Chat", "View Friends", "Send Request" },
        new string[] { "Map"},
        new string[] { "Sound", "Sensitivity", "Voice Chat"},
    };

    private int curSecondaryMenu = -1;
    float transitionConstant = 0.09f;

    private void ButtonMode(Transform button, bool mode)
    {
        if (mode)
        {
            button.GetComponent<Image>().color = new Color32(0, 245, 255, 137);
            for (int i = 0; i < buttons[cur].Length; i++)
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
                catch(Exception e)
                {
                    Debug.Log(e.ToString());
                }
                
            }
        }
        else
        {
            button.GetComponent<Image>().color = new Color32(255, 255, 255, 121);
            extensions.DetachChildren();
        }
    }

    


    public IEnumerator Toggle(int dir) // scrolls through the menu options
    {
        float transitionTime = 0.5f * Mathf.Abs(dir);
        if (!inTranslation && cur + dir > 0 && cur + dir < order.Length) // only toggle if we're not already toggling and if we have room
        {
            inTranslation = true;

            // get the variables
            string oldName = order[cur];
            cur += dir;
            string newName = order[cur];
            float startPos = choices.localPosition.y;
            float translation = dir * transitionConstant;
            float startingoffset = 0;

            ButtonMode(choices.Find(oldName), false); // unmark button and remove old menu

            // translate buttons
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime + transitionTime)
            {
                // this changes the object's position in world space, should be based on camera
                choices.localPosition = new Vector3(choices.localPosition.x,
                    startPos + translation * (Time.realtimeSinceStartup - startTime) / transitionTime  - startingoffset,
                    choices.localPosition.z);
                yield return new WaitForSeconds(0.001f);
            }
            choices.localPosition = new Vector3(choices.localPosition.x, startPos + translation - startingoffset, choices.localPosition.z);

            ButtonMode(choices.Find(newName), true); // show new menu
            inTranslation = false; // allow translation again
        }
        yield return null;

    }
    
    public void Activate(bool value)
    {
        print(value);
        if (value && cur > 0) // reset the values
        {
            ButtonMode(choices.Find(order[cur]), false);
            choices.Translate(new Vector3(0, transitionConstant * cur *-1, 0));
            cur = 0;
        }
        choices.parent.gameObject.SetActive(value);
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

    void clicked(string button)
    {
        Debug.Log(button);

    }


    void Update()
    {

        //ray.rayCalc();

        if (Input.GetKey(KeyCode.K)) // up
        {
            Debug.Log("K value being presed");
            StartCoroutine(Toggle(-1));
        }
        if (Input.GetKey(KeyCode.M)) // down
        {
            StartCoroutine(Toggle(1));
        } if (Input.GetKey(KeyCode.O)) // open/close
        {
            switchSeen();
        }

            //Debug.Log("righy" + collision[0]);
            //Debug.Log("left" + collision[1]);

        }

}
