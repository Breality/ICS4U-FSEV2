using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    public UDPClient udpHandler;
    private List<Transform> cur_collisions = new List<Transform>();
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint cp in collision.contacts)
        {
            if (cp.thisCollider.name.Contains("Hull") && (collision.collider.transform.parent != this.transform.root))//SwordHit && Not Hit Self
            {
                int handed = cp.thisCollider.transform.parent.parent.name == "Swords Left" ? 0 : 1;

                if (isHittable(collision.collider.transform))
                {
                    Debug.Log(handed + " " + collision.collider.transform.root.name);

                    udpHandler.PlayerHit(collision.collider.transform.root.name, handed);

                    break;
                }



            }
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
        else if (objectHit.name.Contains("Hull"))//object hit was sword
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}