using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : DisplayObject
{
    // ------------- variables -------------
    public GameObject Arrows;
    public GameObject Equipments;
    public GameObject EquipParent;
    public InfoCenter Info;    

    // ------------- functions that are called by the MenuToggle raycasting -------------
    public new List<GameObject> Activated()  // returns parents of buttons to look for
    {
        Debug.Log("Equipment Being activated");
        return null;
    }

    public new void Hover(GameObject item)
    {
        Debug.Log("Being told hover on " + item.name);
    }

    public new void UnHover(GameObject item)
    {
        Debug.Log("Being told unhover on " + item.name);
    }

    public new void Clicked(GameObject item)
    {
        Debug.Log("Being told clicked on " + item.name);
    }

    public Equipment()
    {

    }
}
