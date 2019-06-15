using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    private List<Transform> cur_collisions = new List<Transform>();
    [SerializeField]
    Transform rootComponent;
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform.root != this.transform.root)
        {
            cur_collisions.Add(collision.collider.transform);
        }
        
    }
    private void OnCollisionExit(Collision collision)
    {
        
    }
}
