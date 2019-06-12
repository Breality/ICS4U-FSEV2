using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Equipment : DisplayObject
{
    // ------------- Variables -------------
    // don't forget that all DisplayObjects have a variable "info" pointing at the info center
    private static Transform main;
    public GameObject Arrows;
    public GameObject Equipments;
    
    public TMP_Text Title;

    private string[] options = new string[] { "Weapons", "Helmets", "Pendants" , "Armour", "Boots" };
    private int view = 0;

    // ------------- Functions for equipment -------------
    private void ReloadMenu()
    {
        Title.text = options[view];
        Equipments.transform.DetachChildren(); // remake them

        List<string> itemsOwned = Info.inventory[options[view]];
        int i = 0;
        foreach(string item in itemsOwned)
        {
            

            i++;
        }
    }


    // ------------- Functions that are called by the MenuToggle raycasting -------------
    public new List<GameObject> Activated()  // returns parents of buttons to look for
    {
        Debug.Log("Equipment Being activated");
        ReloadMenu();
        return (new List<GameObject> { Arrows, Equipments });
    }

    public new void Hover(GameObject item)
    {

    }

    public new void UnHover(GameObject item)
    {

    }

    private float lastClick = 0; // debounce system
    public new void Clicked(GameObject item)
    {
        if (Time.time - lastClick < 0.5f){ return; }
        lastClick = Time.time;

        if (item.transform.parent.name == "Arrows") // the arrows, toggling
        {
            view = (view + int.Parse(item.name) + options.Length)% options.Length;
            ReloadMenu();
        }
    }

    public Equipment(GameObject camera)
    {
        main = camera.transform.Find("Activated UI").transform.Find("Display").transform.Find("Equipment");
        Arrows = main.Find("Arrows").gameObject;
        Equipments = main.Find("Equipments").gameObject;
        Title = main.Find("Title").gameObject.GetComponent<TMP_Text>();
        Debug.Log("ready for activate");
}
}
