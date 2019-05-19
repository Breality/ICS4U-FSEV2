using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsunaMovement : MonoBehaviour
{
    private Animator asunaController;
    // Start is called before the first frame update
    void Start()
    {
        asunaController = GetComponent<Animator>();
        asunaController.SetFloat("Vx", 0);
        asunaController.SetFloat("Vy", 1);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKey(KeyCode.W))
        {

        }
        */
    }
}
