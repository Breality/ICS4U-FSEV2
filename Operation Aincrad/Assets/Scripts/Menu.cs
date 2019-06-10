using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Menu : MonoBehaviour
{

    // Start is called before the first frame update
    bool leftSwipe = false, rightSwipe = false;
    [SerializeField]
    private float swipeActivateMin;
    Vector3 prevRHand, prevLHand;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rHand = InputTracking.GetLocalPosition(XRNode.RightHand);
        Vector3 lHand = InputTracking.GetLocalPosition(XRNode.LeftHand);
        Debug.Log(rHand);
        if(rHand.y - prevRHand.y > swipeActivateMin)
        {
            rightSwipe = true;
        }
        if(lHand.y - prevLHand.y > swipeActivateMin)
        {
            leftSwipe = true;
        }
        prevLHand = lHand;
        prevRHand = rHand;
        OpenSesami();
    }
    void OpenSesami()
    {
        //Debug.Log(Input.GetButton("R_Menu"));
        if((Input.GetButton("L_Menu")&& leftSwipe)||(Input.GetButton("R_Menu") && rightSwipe))
        {
            Debug.Log("Open Sesami");
        }
        
    }
}
