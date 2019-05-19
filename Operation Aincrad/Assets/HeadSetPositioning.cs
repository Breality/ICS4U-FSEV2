using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HeadSetPositioning : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        this.transform.localPosition = -InputTracking.GetLocalPosition(XRNode.CenterEye);
    }
}
