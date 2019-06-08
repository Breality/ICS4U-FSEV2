using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Delete : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {

        int i = 0;
        foreach (Transform child in this.transform)
        {
            if (i%2 == 0)
            {
                Destroy(child.gameObject);
                
            }
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
