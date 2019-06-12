using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Mirror;
public class HeadsetPositioning : NetworkBehaviour
{

    // Update is called once per frame
    private void Start()
    {
        InputTracking.disablePositionalTracking = true;
        this.transform.localPosition = new Vector3(0, 1.54f, 0.1f);
        if (!isLocalPlayer)
        {
            this.gameObject.SetActive(false);
        }
    }
}
