using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public string type;
    // Start is called before the first frame update
    void Start()
    {
        if(type == "skeleton")
        {
            Debug.Log("move this shit");
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
