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
        foreach(Transform child in parent)
        {
            monsters.Add(child.gameObject);
        }

        if(level == 0)
        {
            //Copy and instantiate new monster
            GameObject m = Instantiate(monsters[0],parent);
            m.transform.localPosition = new Vector3(0, 0, 0);
            m.SetActive(true);
            


        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
