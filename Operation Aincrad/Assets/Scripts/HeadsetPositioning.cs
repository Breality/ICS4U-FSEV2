using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HeadSetPositioning : MonoBehaviour
{

    // Update is called once per frame
    private void Start()
    {
        InputTracking.disablePositionalTracking = true;
        this.transform.localPosition = new Vector3(0, 1.54f, 0.1f);
    }
}
