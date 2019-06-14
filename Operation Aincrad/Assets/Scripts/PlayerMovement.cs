using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * PlayerMovement
 * Description: Takes in joystick movement so the character can move
 */
public class PlayerMovement : NetworkBehaviour
{
    // Start is called before the first frame update
    private Animator charAnim;
    private float vx, vy,rot_y;
    private const float deadZone = 0.5f;
    [SerializeField]
    private float rotation_Speed = 1f;
    [SerializeField]
    private float playerMovSpeed = 0.2f;
    [SerializeField]
    private GameObject cam;
    void Start()
    {
        //This plays the player animations like running or walking or idle
        charAnim = this.gameObject.GetComponent<Animator>();
        
        //If the new spawned object is not local (someone else on multiplayer), these scrips are useless
        if (!isLocalPlayer)
        {
            //Destroy these scripts so they don't interfere with the current player playing
            cam.SetActive(false);
            Destroy(this.GetComponent("AvatarController"));
            Destroy(this.GetComponent<Animator>());
            Destroy(cam.GetComponent("KinectManager"));
        }
                
    }

    // Update is called once per frame
    void Update()
    {

        if (!isLocalPlayer)
            return;

        //Gets the joystick input
        vx = Mathf.Abs(Input.GetAxis("R_Horizontal"))>= deadZone ? Input.GetAxis("R_Horizontal"):0;
        vy = Mathf.Abs(Input.GetAxis("R_Vertical")) >= deadZone ? Input.GetAxis("R_Vertical") : 0;
        rot_y = Mathf.Abs(Input.GetAxis("L_Horizontal")) >= deadZone ? Input.GetAxis("L_Horizontal") : 0;


        //If the player wants to move faster
        if (Input.GetButton("Sprint"))
        {
            vx *= 2;
            vy *= 2;
        }

        if (Input.GetButton("FastRotation"))
        {
            rot_y *= 2;
        }

        //Moves the player and updates their rotation
        this.transform.Rotate(0, rot_y*rotation_Speed, 0);
        this.transform.GetComponent<Rigidbody>().position += this.transform.right * playerMovSpeed*vx;
        this.transform.GetComponent<Rigidbody>().position += this.transform.forward * playerMovSpeed * vy;
        
        //Set the animations for the player
        charAnim.SetFloat("Vx", vx);
        charAnim.SetFloat("Vy", vy);


        




    }


}
