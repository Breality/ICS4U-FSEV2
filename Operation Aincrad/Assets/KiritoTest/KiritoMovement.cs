using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KiritoMovement : MonoBehaviour
{
    private Animator kiritoController;
    float speed = 0.1F;
    // Start is called before the first frame update
    void Start()
    {
        kiritoController = GetComponent<Animator>();
        Debug.Log(Input.GetJoystickNames()[1]);
    }

    // Update is called once per frame
    void Update()
    {
        float vx = Mathf.Abs(Input.GetAxis("L_Horizontal")) > 0.2f ? Input.GetAxis("L_Horizontal") : 0;
        float vy = Mathf.Abs(Input.GetAxis("L_Vertical")) > 0.2F ? Input.GetAxis("L_Vertical") : 0;
        kiritoController.SetFloat("Vx", vx);
        kiritoController.SetFloat("Vy", vy);

        this.transform.position = new Vector3(this.transform.position.x + vx*speed, this.transform.position.y,this.transform.position.z+vy*speed);
    }
}
