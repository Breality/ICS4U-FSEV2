using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Spawn Class
 * Description: Loads in monsters
 */
public class Spawn : MonoBehaviour
{
    public Transform parent;
    public int level;
    // Start is called before the first frame update
    private List<GameObject> monsters = new List<GameObject>();

    void Start()
    {
        Physics.IgnoreLayerCollision(9,9); // Makes sure monsters don't collide with themselves

        //Loads all types of monsters
        foreach (Transform child in parent)
        {
            monsters.Add(child.gameObject);
        }
        
        //Spawns the monsters
        loadM(new Vector3(486, 20, 586), 1, 1);
        loadM(new Vector3(486, 20, 586), 1, 0);
        loadM(new Vector3(486, 20, 586), 1, 2);

    }

    //Spawns a type of monster num times in a position
    public void loadM(Vector3 position,int num, int type)
    {
        for(int i = 0; i < num; i++)
        {
            //GameObject m = Instantiate(monsters[type], parent);
            GameObject m = Instantiate(monsters[type]);
            //Makes sure that the monsters aren't overlapping
            m.transform.position = position + new Vector3(Random.Range(0,5),0,Random.Range(0,5));
            m.SetActive(true);
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
