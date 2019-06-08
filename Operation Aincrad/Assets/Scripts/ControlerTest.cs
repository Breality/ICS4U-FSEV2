using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string[] names = Input.GetJoystickNames();
        Debug.Log("Connected Joysticks:");
        foreach (string stick in names)
        {
            Debug.Log("Joystick " + stick);
        }
    }
}
