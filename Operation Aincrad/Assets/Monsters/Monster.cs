using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public string type;
    // Start is called before the first frame update
    private Animator charAnim;
    public Transform thing;
    private List<Vector3> test = new List<Vector3>();
    private float minAttackDist = 0.8f;
    private bool isDead = false;
    private float speed = 2f;
    public float HP = 100f;


    void Start()
    {

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

    //Retardism
    //Attack
    //Attack patterns

    //You need to call this one in the update loop

    public void chase(List<Vector3> positions)
    {
        float distance = 99999;
        Vector3 position = this.transform.position;
        foreach (Vector3 p in positions)
        {
            if (Vector3.Distance(p, this.transform.position) < distance)
            {
                distance = Vector3.Distance(p, this.transform.position);
                position = p;
                Debug.Log("works");
            }
        }

        
        if (minAttackDist < Vector3.Distance(position, this.transform.position))
        {
            charAnim.SetFloat("velocity", 1);
            transform.LookAt(position);


            transform.position += transform.forward * speed * Time.deltaTime;
            
        }
        else
        {
            charAnim.SetFloat("velocity", 0);
            attack();


        }

    }
    

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Take 0.001 damage!");
    }

    public void die()
    {
        charAnim.SetTrigger("Die");
        isDead = true;
        
    }



    public void attack()
    {
        charAnim.SetTrigger("Attack");

        //check

    }

    // Update is called once per frame
    void Update()
    {

        


        //Debug.Log(Vector3.Distance(position,this.transform.position));
        Vector3 rot = transform.localEulerAngles;
        rot.x = 0;
        rot.z = 0;
        transform.localEulerAngles = rot;

        test = new List<Vector3>();
        test.Add(thing.position);
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

    }
}