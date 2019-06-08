using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator charAnim;
    private float vx, vy,rot_y;
    private const float deadZone = 0.5f;
    [SerializeField]
    private float rotation_Speed = 1f;
    [SerializeField]
    private float playerMovSpeed = 0.2f;

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
        vx = Mathf.Abs(Input.GetAxis("R_Horizontal"))>= deadZone ? Input.GetAxis("R_Horizontal"):0;
        vy = Mathf.Abs(Input.GetAxis("R_Vertical")) >= deadZone ? Input.GetAxis("R_Vertical") : 0;

        rot_y =Mathf.Abs(Input.GetAxis("L_Horizontal")) >= deadZone ? Input.GetAxis("L_Horizontal") : 0;
        this.transform.Rotate(0, rot_y*rotation_Speed, 0);
        

        this.transform.GetComponent<Rigidbody>().position += this.transform.right * playerMovSpeed*vx;
        this.transform.GetComponent<Rigidbody>().position += this.transform.forward * playerMovSpeed * vy;

            
        Debug.Log(vx + " " + vy);
        charAnim.SetFloat("Vx", vx);
        charAnim.SetFloat("Vy", vy);

    }


}
