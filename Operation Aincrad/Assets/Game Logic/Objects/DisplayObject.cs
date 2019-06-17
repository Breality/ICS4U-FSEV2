/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * DisplayObject class
 * Description:
 * Like equipment, there are many displayable menus. They all need to be lumped together in a list, so instead this DisplayObject class is used in the list and they all inherit from it
 */

// Importing modules  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayObject  // class exists to link all the display objects in one dataset,  each override these functions differantly
{
    // public variables that all display objects will use
    public InfoCenter Info = GameObject.Find("InfoCenter").GetComponent<InfoCenter>();
    public HTTPClient HTTP = GameObject.Find("HTTP Handler").GetComponent<HTTPClient>();

    // functions that will be called by the menu toggle, and instead go through to the inherited items
    public List<GameObject> Activated() 
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
