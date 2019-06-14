using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public Transform parent;
    public int level;
    // Start is called before the first frame update
    private List<GameObject> monsters = new List<GameObject>();

    void Start()
    {
        Physics.IgnoreLayerCollision(9,9);
        foreach (Transform child in parent)
        {
            monsters.Add(child.gameObject);
        }

        if(level == 0)
        {
            //Copy and instantiate new monster
            for(int i = 0; i< 2; i++)
            {
                GameObject m = Instantiate(monsters[i], parent);
                m.transform.position = new Vector3(i, 0, 0);
                m.SetActive(true);
            }
            
            


        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
