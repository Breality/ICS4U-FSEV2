using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float x;
    public float z;
    public float y;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.UpArrow))
        {
            x -= Time.deltaTime * 50;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            x += Time.deltaTime * 50;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            y += Time.deltaTime * 50;
            
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            y -= Time.deltaTime * 50;
        }
        transform.rotation = Quaternion.Euler(x, y, z);



    }
}
