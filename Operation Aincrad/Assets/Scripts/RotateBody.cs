using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * RotateBody.cs
 * Description: Updates player rotation based off camera rotation. (Less Shakiness)
 */
public class RotateBody : MonoBehaviour
{
    [SerializeField]
    Transform cam;
    void LateUpdate()//used late update to bypass animations
    {
        Vector3 rot = this.transform.localEulerAngles;//get rotation of this object
        rot.y = cam.localEulerAngles.y;//set it to be the same as the camera rotation
        this.transform.localEulerAngles = rot;//apply rotation
    }
}
