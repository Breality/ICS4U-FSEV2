using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float vx = Input.GetAxis("Horizontal");
        Debug.Log(vx);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
