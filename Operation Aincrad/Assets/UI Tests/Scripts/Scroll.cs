using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{
    public Transform display;
    public List<GameObject> itemList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void Begin(string type, Transform parent,string style)
    {
        /*
        if (type == "Inventory")
        {
            foreach(Transform item in parent)
            {
                //Show it 
                itemList.Add(item.gameObject);
            }
            GameObject thing = Instantiate(itemList[0], display);
            thing.transform.position = new Vector3(0, 0, 0);

        }
        */
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
