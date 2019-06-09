using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;

public class Spin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    

    // Update is called once per frame
    float x;
    void Update()
    {
        x -= Time.deltaTime * 20;
        transform.rotation = Quaternion.Euler(0, x, 0);

    }
}
