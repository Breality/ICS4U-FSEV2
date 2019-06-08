using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator charAnim;

    void Start()
    {
        charAnim = this.gameObject.GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        /*string[] names = Input.GetJoystickNames();
        Debug.Log("Connected Joysticks:");
        foreach (string stick in names)
        {
            Debug.Log("Joystick " + stick);
        }*/
        float vx = Input.GetAxis("L_Horizontal");
        float vy = Input.GetAxis("L_Vertical");
        Debug.Log(vx + " " + vy);
        charAnim.SetFloat("Vx", vx);
        charAnim.SetFloat("Vy", vy);

    }

}
