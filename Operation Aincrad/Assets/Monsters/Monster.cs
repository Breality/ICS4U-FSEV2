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



    void Start()
    {

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
        foreach (Vector3 p in positions)
        {
            if (Vector3.Distance(p, this.transform.position) < distance)
            {
                distance = Vector3.Distance(p, this.transform.position);
                position = p;
                Debug.Log("works");
            }
        }

        //Debug.Log(Vector3.Distance(position,this.transform.position));
        Vector3 rot = transform.localEulerAngles;
        rot.x = 0;
        rot.z = 0;
        transform.localEulerAngles = rot;
        if (minAttackDist < Vector3.Distance(position, this.transform.position))
        {


            //this.transform.position += diff * (0.01f); // This makes it slow down near the end but whatever
            transform.LookAt(position);


            transform.position += transform.forward * 0.05f;


            //transform.eulerAngles = new Vector3(0, transform.rotation.y, 0);

            //transform.rotation = Quaternion.LookRotation(relativePos);
        }

        if (1 < Vector3.Distance(position, this.transform.position))
        {
            charAnim.SetFloat("velocity", 1);
        }

        else
        {
            charAnim.SetFloat("velocity", 0);
            attack();


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


        test = new List<Vector3>();
        test.Add(thing.position);
        chase(test);

    }
}