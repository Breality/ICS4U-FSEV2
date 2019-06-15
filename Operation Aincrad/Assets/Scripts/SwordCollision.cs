using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    private List<Transform> cur_collisions = new List<Transform>();
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Sword Hit: " + collision.collider.name);
        if (collision.collider.transform.root != this.transform.root && isHittable(collision.collider.transform)) //not same player and object is hittable
        {
            Debug.Log("SWORD HIT :"+collision.collider.transform.root.name);
            cur_collisions.Add(collision.collider.transform);
        }
        
    }
    private void OnCollisionExit(Collision collision)
    {
        
    }
    private bool isHittable(Transform objectHit)
    {
        if (objectHit.root.GetChild(0).Find("Asuna_def_001") == null)//not player NEED TO CHANGE FOR MONSTERS
        {
            return false;
        }
        else if(objectHit.parent!=null && objectHit.parent.parent!=null && objectHit.parent.parent.name.Contains("Swords"))//object hit was sword
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
