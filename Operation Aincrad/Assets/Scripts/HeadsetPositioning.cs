using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Mirror;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * HeadSet Positioning
 * Description: Since the headset allows for positional movement, we want the headset to stay on the character
 */
public class HeadsetPositioning : MonoBehaviour
{

    // Update is called once per frame
    private void Start()
    {
        //Disable XR tracking
        InputTracking.disablePositionalTracking = true;
        //Positions camera correctly
        this.transform.localPosition = new Vector3(0, 1.54f, 0);

    }
}
