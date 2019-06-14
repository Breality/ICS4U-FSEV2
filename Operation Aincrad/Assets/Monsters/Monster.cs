using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public string type;
    // Start is called before the first frame update
    private Animator charAnim;
<<<<<<< HEAD
    public Transform player;
    private List<Vector3> test = new List<Vector3>();
   

    void Start()
    {
        test.Add(player.transform.position);
=======
    public Transform thing;
    private List<Vector3> test = new List<Vector3>();
    private float minAttackDist = 0.7f;
    


    void Start()
    {
        
>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
        charAnim = this.gameObject.GetComponent<Animator>();
        if (type == "skeleton")
        {
            Debug.Log("move this shit");
            
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
        foreach(Vector3 p in positions)
        {
            if (Vector3.Distance(p, this.transform.position) < distance)
            {
                distance = Vector3.Distance(p, this.transform.position);
                position = p;
                Debug.Log("works");
            }
        }
<<<<<<< HEAD
        
        if (0.7 < Vector3.Distance(position, this.transform.position))
=======

        //Debug.Log(Vector3.Distance(position,this.transform.position));
        Vector3 rot = transform.localEulerAngles;
        rot.x = 0;
        rot.z = 0;
        transform.localEulerAngles = rot;
        if (minAttackDist < Vector3.Distance(position, this.transform.position))
>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
        {
            
            
            //this.transform.position += diff * (0.01f); // This makes it slow down near the end but whatever
            transform.LookAt(position);
<<<<<<< HEAD
            Vector3 rot = transform.localEulerAngles;
            rot.x = 0;
            rot.z = 0;
            transform.localEulerAngles = rot;
=======

           
>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
            transform.position += transform.forward*0.05f;
            
            
            //transform.eulerAngles = new Vector3(0, transform.rotation.y, 0);

            //transform.rotation = Quaternion.LookRotation(relativePos);
        }

        if(1 < Vector3.Distance(position, this.transform.position))
        {
            charAnim.SetFloat("velocity", 1);
        }

        else
        {
            charAnim.SetFloat("velocity", 0);
            attack();
<<<<<<< HEAD
=======

>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
            
        }
        
    }
    /*
    public IEnumerator chase(Vector3 position)
    {
        while(1 < Vector3.Distance(position, this.transform.position))
        {
            this.transform.position += transform.forward * 0.02f;
            transform.LookAt(position);
            yield return new WaitForSeconds(0.001f);
        }
        
    }
    */

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Take 0.001 damage!");
    }



    public void attack()
    {
        charAnim.SetTrigger("Attack");
        
        //check

    }








    public void attack()
    {
        charAnim.SetTrigger("Attack");
        
        //check

    }








    // Update is called once per frame
    void Update()
    {
        /*
        if(type == "skeleton")
        {
        }
        
        this.transform.position += transform.forward*0.02f;
        this.transform.rotation *= Quaternion.Euler(new Vector3(0, 1, 0));
        */


<<<<<<< HEAD
        test[0] = player.transform.position;
        chase(test);
=======
    test = new List<Vector3>();
    test.Add(thing.position);
    chase(test);
>>>>>>> 5471fe37d0b776fa448d4e0633b5861f00c25cdb
        
    }
}
