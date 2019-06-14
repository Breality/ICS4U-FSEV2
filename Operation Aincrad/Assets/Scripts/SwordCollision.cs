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
        Transform rootPCol= FindParentWithRoot(collision.collider.transform);
        if (rootPCol != null && rootComponent != rootPCol)
        {
            cur_collisions.Add(collision.collider.transform);
        }
        
    }
    private void OnCollisionExit(Collision collision)
    {
        
    }
    public Transform FindParentWithRoot(Transform child)
    {
        Transform t = child;
        while (t.parent != null)
        {
            if(t.parent.name == "Root")
            {
                return t.parent;
            }
            t = t.parent.transform;
        }
        return null;
    }
}
