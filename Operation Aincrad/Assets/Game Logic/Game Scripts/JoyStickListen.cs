/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * JoyStickListen class
 * Description:
 * This class listens to the joy stick's menu button (the side trigger) and opens/closes the correct menu or shop
 */

 // Importing modules
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoyStickListen : MonoBehaviour 
{
    // Object references
    public GameObject character; // the character of our VR person
    public GameObject sellers; // the parent of all the sellers

    // UI menus that get opened/closed 
    public GameObject mainMenu;
    public GameObject weaponSeller;
    public GameObject helmetSeller;
    public GameObject armourSeller;
    public GameObject bootSeller;
    public GameObject pendantSeller;

    // UI Button that shows up to instruct them on opening shop
    public GameObject menuReveal;

    // private variables
    private static Dictionary<string, int> UIindexs = new Dictionary<string, int> {  { "Weapon Seller", 1 }, // pointing the seller npcs to the correct UIs 
    { "Helmet Seller", 2 }, { "Armour Seller", 3 }, { "Boot Seller", 4 }, { "Pendant Seller", 5 }}; 
    private string[] namesShown = new string[] {null,  "Weapons", "Helmets", "Armour", "Boots", "Pendants" }; // the names shown to the user on the menuReveal button
    private GameObject[] UIs; // This is the list of interfaces they can open/close

    private int menuHover = 0; // keeping track of which menu can be opened, according to how close they are to npcs.
    private float lastUsed = 0; // debounce system

    private void Start() 
    {
        // setting init variables
        UIs = new GameObject[] { mainMenu, weaponSeller, helmetSeller, armourSeller, bootSeller, pendantSeller }; // in the order that menuHover UIindexs in
    }
    
    void Update() // checking for input and changing menus
    {
        // check for position changes
        int newSelect = 0;
        for (int i=0; i < sellers.transform.childCount; i++) // loop through all sellers and figure out if we are close enough to any sellers
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
            // remove old menu if it is open
            GameObject item = UIs[menuHover];
            item.SetActive(false);

            // check for new menu
            menuHover = newSelect;
            if (menuHover > 0) // show them text so they know they can open a shop
            {
                menuReveal.transform.Find("TEXT").GetComponent<TMP_Text>().text = namesShown[menuHover];
                menuReveal.SetActive(true);
            }
            else // hide the text because they are not close enough to any npcs
            {
                menuReveal.SetActive(false);
            }
        }

        // check for gripping with account to the debounce system
        if (Input.GetButton("L_Grip") && Time.time - lastUsed > 0.5f) 
        {
            Debug.Log("They clicked it");
            lastUsed = Time.time;
            GameObject item = UIs[menuHover]; // shop or npc menu

            item.SetActive(!item.activeSelf); // change the mode on the current menu
            if (menuHover > 0) // account for instructions when close enough to sellers
            {
                menuReveal.SetActive(!menuReveal.activeSelf); // hide the instructions when they open the shop and reveal again when they close the shop
            }
        }
    }
}
