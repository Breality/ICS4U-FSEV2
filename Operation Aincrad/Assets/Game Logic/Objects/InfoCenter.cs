using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoCenter : MonoBehaviour
{
    // basic info
    public string username;
    public int gold = 0;
    public int score = 0;

    // equipment in {name : count}
    public Dictionary<string, int> clothing;
    public Dictionary<string, int>  weapons;
    public Dictionary<string, int>  items;

   


    public string[] equipped = new string[6] { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Default Sword", "None" };

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
