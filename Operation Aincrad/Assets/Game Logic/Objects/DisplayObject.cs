using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayObject  // class exists to link all the display objects in one dataset,  each override these functions differantly
{
    public List<GameObject> Activated()  // returns parents of buttons to look for
    {
        return null;
    }

    public void Hover(GameObject item)
    {

    }
    public void UnHover(GameObject item)
    {

    }

    public void Clicked(GameObject item)
    {

    }

    public DisplayObject()
    {

    }
}
