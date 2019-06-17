using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * CameraSpin.cs
 * Description: Rotates the camera so it looks around the map - for the intro screen
 */
public class CameraSpin : MonoBehaviour
{
    
    IEnumerator spin()//new thread that runs along other codes
    {
        while (true)//Will continue rotating forever until login cam is disabled via login/register
        {
            //Rotates the camera
            transform.Rotate(new Vector3(0, 0.15f, 0));
            yield return new WaitForSeconds(0.01f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Starts a new thread so it doesn't interrupt any other processes (e.g login)
        StartCoroutine(spin());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
