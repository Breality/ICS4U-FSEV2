using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetPositioning : MonoBehaviour
{
    // Start is called before the first frame update
    Vector3 prevCamPos;
    public CharacterMovement player;
    void Start()
    {
        prevCamPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (prevCamPos != this.transform.localPosition)
        {
            player.setRelativeCamPos(this.transform.localPosition - prevCamPos);
            prevCamPos = this.transform.localPosition;
        }


    }
}
