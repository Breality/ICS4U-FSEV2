using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillHouses : MonoBehaviour
{
    public Transform p;
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        foreach(Transform d in p)
        {
            if(i%2 == 0)
            {
                Destroy(d.gameObject);
            }
            i += 1;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
