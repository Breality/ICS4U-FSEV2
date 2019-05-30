using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour
{

    public List<GameObject> generate = new List<GameObject>();
    public Transform p;



    // Start is called before the first frame update
    void Start()
    {
        //X position within 200 - 700
        //Z - 300 -700
        // y = 20
        //randomize orientation 0, 90, 180, 270 for y rotation
        //Space them 20 apart
        //Once done, delete some and add in unique shops
        foreach(Transform child in p)
        {
            generate.Add(child.gameObject);
        }

        //GameObject building = Instantiate(generate[0], p);
        //building.transform.position = new Vector3(200, 20, 200);
        //building.transform.Rotate(new Vector3(0, 90, 0));


        for(int x = 200; x<800; x += 20)
        {
            for (int z = 300; z < 800; z += 20)
            {
                GameObject building = Instantiate(generate[Random.Range(0, generate.Count - 1)], p);
                building.transform.position = new Vector3(x, 20, z);
                building.transform.Rotate(new Vector3(0, Random.Range(0, 3) * 90));
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
