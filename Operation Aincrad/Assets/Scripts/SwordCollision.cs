using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * SwordCollision.cs
 * Description: Checks for collisions of the sword and tells server what happens during battle.
 */
public class SwordCollision : MonoBehaviour
{
    public UDPClient udpHandler;//used to send messages of hits to the server
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)//when a collision enters an object
    {
        foreach (ContactPoint cp in collision.contacts) //find contact points so we can tell what part of the player hit collided
        {
            if (cp.thisCollider.name.Contains("Hull") && (collision.collider.transform.parent!=this.transform.root))//if the sword hit another object that was not the person's own character continue checking for collisions

            {
                int handed = cp.thisCollider.transform.parent.parent.name == "Swords Left" ? 0 : 1;//get hand that was used (to tell server which weapon was used and what damage should be assigned)

                if (isHittable(collision.collider.transform))//if object hit is valid (Variety of Criteria)
                {

                    udpHandler.PlayerHit(collision.collider.transform.root.name, handed);//send server message of what was hit and the hand used to hit it

                    break;//only one hit per frame (no overkill)
                }

            }
        }

    }
    private bool isHittable(Transform objectHit)//checks if object hit was valid (more to be added)
    {
        if (objectHit.root.GetChild(0).Find("Asuna_def_001") == null)//if object hit was not a player do nothing (Monsters Not Added)
        {
            return false;
        }
        else if (objectHit.name.Contains("Hull"))//(Object Hit Was Sword - Parry)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}