using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Generate
 * Description: Used initially when we were developing the map to randomize spawned buildings
 */

public class Generate : MonoBehaviour
{

    public List<GameObject> generate = new List<GameObject>();
    public Transform p;



    // Start is called before the first frame update
    void Start()
    {
        //Takes the unique buildings stored in an empty game object (holds multiple building models)
        foreach(Transform child in p)
        {
            generate.Add(child.gameObject);
        }

        

        //Across the map, add these buildings
        for(int x = 200; x<800; x += 20)
        {
            for (int z = 300; z < 800; z += 20)
            {
                //Randomize position, rotation
                GameObject building = Instantiate(generate[Random.Range(0, generate.Count - 1)], p);
                building.transform.position = new Vector3(x, 20, z);
                building.transform.Rotate(new Vector3(0, Random.Range(0, 3) * 90));
            }
        }


    }

    
}
