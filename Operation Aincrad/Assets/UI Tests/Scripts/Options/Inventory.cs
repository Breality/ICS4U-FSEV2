using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<string,List<string>> getInfo;
    public Transform swords;
    public Transform hats;
    public Transform boots;
    public Transform body;
    public Transform pendant;
    public Transform other;
    // Start is called before the first frame update
    private List<string> itemType = new List<string> { "Sword", "Hat", "Boots", "Body", "Pendant", "Other" };
    void OnEnable()
    {
        // Do however it is rn
        Debug.Log("Show inventory");
        //Show it with 4 every row.
        foreach(string section in itemType)
        {
            private List<string> items = getInfo[section];

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
