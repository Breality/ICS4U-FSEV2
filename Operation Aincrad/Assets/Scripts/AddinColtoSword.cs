using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddinColtoSword : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform swordHolder = this.transform;
        foreach(Transform sword in swordHolder)
        {
            foreach(Transform hull in sword.Find("Hulls"))
            {
                hull.gameObject.AddComponent<SwordCollision>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
