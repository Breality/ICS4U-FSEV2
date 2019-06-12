using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoyStickListen : MonoBehaviour
{
    // Object references
    public GameObject character;
    public GameObject sellers;

    // UI references 
    public GameObject mainMenu;
    public GameObject weaponsSeller;
    public GameObject menuReveal;

    // private variables
    private static Dictionary<string, int> UIindexs = new Dictionary<string, int> {  { "Weapon Seller", 1 } }; // put whatever npc they are and which UI index they point to
    private string[] namesShown = new string[] {null,  "Weapons" };
    private GameObject[] UIs;
    private int menuHover = 0;
    private float lastUsed = 0; // debounce

    private void Start() // setting init variables
    {
        UIs = new GameObject[] { mainMenu, weaponsSeller }; // in the order that menuHover UIindexs in
    }
    
    void Update() // checking for input and changing menus
    {
        // check for position changes
        int newSelect = 0;
        for (int i=0; i < sellers.transform.childCount; i++)
        {
            Transform seller = sellers.transform.GetChild(i);

            if (Vector3.Distance(seller.position, character.transform.position) <= 2)
            {
                Debug.Log("Close enough to " + seller.name);
                newSelect = UIindexs[seller.name];
            }
        }

        if (newSelect != menuHover) // walked into a new potential menu
        {
            // remove old menu if it exists
            GameObject item = UIs[menuHover];
            item.SetActive(false);

            // check for new one
            menuHover = newSelect;
            if (menuHover > 0) // show them text
            {
                menuReveal.transform.Find("TEXT").GetComponent<TMP_Text>().text = namesShown[menuHover];
                menuReveal.SetActive(true);
            }
            else
            {
                menuReveal.SetActive(false);
            }
        }


        // check for gripping
        if (Input.GetButton("L_Grip") && Time.time - lastUsed > 0.5f) // shop or npc
        {
            Debug.Log("They clicked it");
            lastUsed = Time.time;
            GameObject item = UIs[menuHover];
            item.SetActive(!item.activeSelf);
            if (menuHover > 0)
            {
                menuReveal.SetActive(!menuReveal.activeSelf);
            }
        }
    }
}
