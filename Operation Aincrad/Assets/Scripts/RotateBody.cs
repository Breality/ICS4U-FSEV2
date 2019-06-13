using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBody : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Transform cam;
    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 rot = this.transform.localEulerAngles;
        rot.y = cam.localEulerAngles.y;
        this.transform.localEulerAngles = rot;
    }
}
