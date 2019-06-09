using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR;
public class Equipment : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform leftHand, rightHand;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        itemHandPos(leftHand, XRNode.LeftHand);
        itemHandPos(rightHand, XRNode.RightHand);
    }
    
    void itemHandPos(Transform hand,XRNode handN)
    {
        if (hand.childCount == 1)
        {
            Transform HandItem = hand.GetChild(0);
            HandItem.rotation = InputTracking.GetLocalRotation(handN);

        }
    }
}

