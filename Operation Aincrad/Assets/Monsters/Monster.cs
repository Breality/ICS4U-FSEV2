using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Monster Class
 * Description: How the monster interacts with in the game
 */
public class Monster : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator charAnim;
    public string type;
    private List<Vector3> test = new List<Vector3>();
    private float minAttackDist = 0.8f;
    private bool isDead = false;
    private float speed = 2f;
    public float HP = 100f;

    void Start()
    {

        //Called whenever a monster gets spawned
        //Different settings for different monsters
        charAnim = this.gameObject.GetComponent<Animator>();
        if (type == "skeleton")
        {
            speed = 3f;
            HP = 20f;
        }
        else if (type == "goblin")
        {
            speed = 2f;
            HP = 50f;
        }
        else if (type == "demon")
        {
            speed = 6f;
            HP = 80f;
        }
        else if (type == "boss")
        {
            speed = 3f;
            HP = 200f;
        }
    }

    //Chases the player
    public void chase(List<Vector3> positions)
    {
        //Gets the player that's closest to them
        float distance = 99999;
        Vector3 position = this.transform.position;
        foreach (Vector3 p in positions)
        {
            if (Vector3.Distance(p, this.transform.position) < distance)
            {
                distance = Vector3.Distance(p, this.transform.position);
                position = p;
            }
        }
        if(distance == 9999)
        {
            charAnim.SetFloat("velocity", 0);
        }
        //Debug.Log(distance);
        if (distance < 40)
        {
            //Move the monster to the player position
            if (minAttackDist < Vector3.Distance(position, this.transform.position))
            {
                Debug.Log("Running");
                charAnim.SetFloat("velocity", 1);
                transform.LookAt(position);
                transform.position += transform.forward * speed * Time.deltaTime;
            }
            //If the monster is in attacking range, play the attack animation
            else if (minAttackDist >= Vector3.Distance(position, this.transform.position))
            {
                charAnim.SetFloat("velocity", 0);
                attack();
            }
        }
        
    }
    
    public void isHit(float damage)
    {
        HP -= damage;
        Debug.Log("Monster has taken damage!");
    }


    //When a monster collides with the player, take damage
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            HP -= 10;
        }
        Debug.Log("Take  damage!");
    }

    //Once the monster's health reaches 0, die
    public void die()
    {
        charAnim.SetTrigger("Die");
        isDead = true;
        
    }



    public void attack()
    {
        charAnim.SetTrigger("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<Vector3> positions = new List<Vector3>();
        foreach(GameObject p in players)
        {
            positions.Add(p.transform.position);
        }
        chase(positions);

        //Makes sure the monster is facing horizontally
        Vector3 rot = transform.localEulerAngles;
        rot.x = 0;
        rot.z = 0;
        transform.localEulerAngles = rot;

        
        if (!isDead && !charAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            Debug.Log("Shouldn't be during attack");
            chase(test);
        }

        if(isDead && charAnim.GetCurrentAnimatorStateInfo(0).IsName("Done"))
        {
            Destroy(gameObject);
            Debug.Log("Monster destroyed");
        }
        if (HP < 0)
        {
            die();
        }

    }
}