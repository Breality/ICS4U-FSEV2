using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Equipment Class
 * Description:
 * This class handles the equipment toggling
 */

public class Equipment : DisplayObject
{
    // ------------- Variables -------------
    // don't forget that all DisplayObjects have a variable "info" pointing at the info center
    private static Transform main;
    public GameObject Arrows;
    public GameObject Equipments;
    public GameObject Template;
    public TMP_Text Title;
    public GameObject weaponSpecs;
    public GameObject clothingSpecs;
    private string[] options = new string[] { "Weapons", "Helmets", "Pendants" , "Armour", "Boots" };
    private int view = 0;
    private UpdatePlayer upHandler;

    // ------------- Functions for equipment -------------
    //Resetting the menu for every time you open it up
    private void ReloadMenu()
    {
        Title.text = options[view];
        //Removes the current option
        foreach (Transform child in Equipments.transform) { UnityEngine.Object.Destroy(child.gameObject); }
        
        //Shows the items you own
        List<string> itemsOwned;
        if (view == 0) // weapons
        {
            itemsOwned = new List<string>(Info.weapons.Keys);
        } else {
            itemsOwned = new List<string>(Info.clothing[options[view]].Keys);
        }

        int i = 0;
        foreach(string item in itemsOwned)
        {
            // new item
            GameObject newItem = UnityEngine.Object.Instantiate(Template);
            newItem.name = item;

            // New Positions
            newItem.transform.parent = Equipments.transform;
            newItem.transform.rotation = Template.transform.rotation;
            newItem.transform.localScale = new Vector3(0.002f, 0.004f, 1);
            newItem.transform.localPosition = new Vector3(0.35f * (i % 3 - 1), (i >= 3) ? -0.35f : 0.35f, Template.transform.localPosition.z);

            // Show product
            Sprite image = Resources.Load<Sprite>("Equipment Images/" + options[view] + " Dealer/" + item);
            newItem.GetComponent<Image>().sprite = image;
            newItem.SetActive(true);
            i++;
        }

        //Shows the stats of the item
        weaponSpecs.SetActive(view == 0);
        clothingSpecs.SetActive(!(view == 0));
    }
    

    // ------------- Functions that are called by the MenuToggle raycasting -------------
    public new List<GameObject> Activated()  // returns parents of buttons to look for
    {
        Debug.Log("Equipment Being activated");
        ReloadMenu();
        return (new List<GameObject> { Arrows, Equipments });
    }

    string selectedItem = null;

    //Should show the item on the primary screen on the right
    public new void Hover(GameObject item)
    {
        Debug.Log("We have recieved the hover request on " + item.name);
        // show new item and stats
        if (item.transform.parent.name == "Equipments") {
            // Display the new hovered item's info
            try
            {
                selectedItem = item.name;
                GameObject specs = view == 0 ? weaponSpecs : clothingSpecs;
                specs.transform.Find("Selected Name").GetComponent<TMP_Text>().text = item.name;
                specs.transform.Find("Selected Image").GetComponent<Image>().sprite = item.GetComponent<Image>().sprite;

                // Displaying the item specs
                if (view == 0) // weapons
                {
                    Weapon weapon = Info.weapons[item.name];
                    specs.transform.Find("Attack").GetComponent<TMP_Text>().text = "Attack: " + weapon.attack;
                    specs.transform.Find("Pierce").GetComponent<TMP_Text>().text = "Pierce: " + weapon.pierce;
                    specs.transform.Find("Range").GetComponent<TMP_Text>().text = "Range: " + weapon.range;
                    specs.transform.Find("Hand Positioning").GetComponent<TMP_Text>().text = new string[] { "Left Handed", "Right Handed", "Dual Weild" }[weapon.weaponType];
                }
                else // Other equipment like armour, boots, etc
                {
                    Debug.Log("Hovering on clothing");
                    Clothing clothing = Info.clothing[options[view]][item.name];
                    specs.transform.Find("Health").GetComponent<TMP_Text>().text = "HP: +" + clothing.bonusHp;
                    specs.transform.Find("Mana").GetComponent<TMP_Text>().text = "Mana: +" + clothing.bonusMana;
                    specs.transform.Find("Stamina").GetComponent<TMP_Text>().text = "Stamina: +" + clothing.bonusStamina;

                    specs.transform.Find("Attack").GetComponent<TMP_Text>().text = "AP: + " + clothing.attackPower[0] + " (x" + clothing.attackPower[1] + ")";
                    specs.transform.Find("Magic").GetComponent<TMP_Text>().text = "MP: + " + clothing.magicPower[0] + " (x" + clothing.magicPower[1] + ")";
                    specs.transform.Find("Speed").GetComponent<TMP_Text>().text = "Speed: +" + clothing.bonusSpeed;
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    //Unhovers, which doesn't give it any effect
    public new void UnHover(GameObject item)
    {

    }

    private float lastClick = 0; // Debounce system

    //Equipping an equipment
    public new void Clicked(GameObject item)
    {
        Debug.Log(selectedItem + " has been clicked, time to switcharoo!");
        if (Time.time - lastClick < 0.5f){ return; }
        lastClick = Time.time;

        if (item.transform.parent.name == "Arrows") // the arrows, toggling
        {
            view = (view + int.Parse(item.name) + options.Length)% options.Length;
            ReloadMenu();

        }

        else if (selectedItem != null) // They want to equip new item (they have nothing equipped right now)
        {
            if (view == 0) // Changing weapons
            {
                Weapon newWeapon = Info.weapons[selectedItem]; // { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Rusty Sword", "None" };
                Debug.Log(newWeapon);
                if (newWeapon.weaponType == 0) { // Left hand
                    //If there is already a weapon, then remove that weapon
                    if (Info.equipped[4] != "None") { Info.WeaponsL.transform.Find(Info.equipped[4]).gameObject.SetActive(false); }
                    if (Info.equipped[5] != "None") { Info.WeaponsR.transform.Find(Info.equipped[5]).gameObject.SetActive(false); }
                    //Add the new equipped weapon
                    Info.WeaponsL.transform.Find(selectedItem).gameObject.SetActive(true);
                    Info.equipped[4] = selectedItem;
                } 
                else if (newWeapon.weaponType == 1) { // Right hand
                    //If there is already a weapon, then remove that weapon
                    if (Info.equipped[5] != "None") { Info.WeaponsR.transform.Find(Info.equipped[5]).gameObject.SetActive(false); }
                    if (Info.equipped[4] != "None") { Info.WeaponsL.transform.Find(Info.equipped[4]).gameObject.SetActive(false); }
                    //Add the new equipped weapon
                    Info.WeaponsR.transform.Find(selectedItem).gameObject.SetActive(true);
                    Info.equipped[5] = selectedItem;

                } 
                else if (newWeapon.weaponType == 2) // Both hands
                {
                    //If there is already a weapon, then remove that weapon
                    if (Info.equipped[5] != "None") { Info.WeaponsR.transform.Find(Info.equipped[5]).gameObject.SetActive(false); }
                    if (Info.equipped[4] != "None") { Info.WeaponsL.transform.Find(Info.equipped[4]).gameObject.SetActive(false); }
                    //Add the new equipped weapon
                    Info.WeaponsR.transform.Find(selectedItem).gameObject.SetActive(true);
                    Info.equipped[5] = selectedItem;
                    Info.WeaponsL.transform.Find(selectedItem).gameObject.SetActive(true);
                    Info.equipped[4] = selectedItem;
                }


                HTTP.AskServer(new Dictionary<string, string> { {"request",  "equip" },
                    {"equipment type",  "Weapon"}, {"equipment name", selectedItem} });
                upHandler.UpdateEquip();
                Debug.Log("Request sent?");

            }
            else
            {
                Clothing newClothing = Info.clothing[options[view]][selectedItem]; // { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Rusty Sword", "None" };
                Debug.Log(newClothing);
                int correspondingIndex = (new Dictionary<string, int> { { "Helmets", 0 },
                    {"Armour", 1 }, { "Boots", 2 }, { "Pendants", 3 } })[options[view]];
                
                if (correspondingIndex == 0)
                {
                    if (Info.equipped[correspondingIndex] != "Default Helmet") { Info.Helmets.transform.Find(Info.equipped[correspondingIndex]).gameObject.SetActive(false); }
                    Info.Helmets.transform.Find(selectedItem).gameObject.SetActive(true);
                }
                else if (correspondingIndex == 1)
                {
                    if (Info.equipped[correspondingIndex] != "Default Armour") { Info.Armour.transform.Find(Info.equipped[correspondingIndex]).gameObject.SetActive(false); }
                    Info.Armour.transform.Find(selectedItem).gameObject.SetActive(true);
                }
                else if (correspondingIndex == 2)
                {
                    //equip left and right
                    if (Info.equipped[correspondingIndex] != "Default Boots") {
                        Info.BootsLeft.transform.Find(Info.equipped[correspondingIndex]).gameObject.SetActive(false);
                        Info.BootsRight.transform.Find(Info.equipped[correspondingIndex]).gameObject.SetActive(false);
                    }
                    Info.BootsLeft.transform.Find(selectedItem).gameObject.SetActive(true);
                    Info.BootsRight.transform.Find(selectedItem).gameObject.SetActive(true);
                }
                else if (correspondingIndex == 3)
                {
                    if (Info.equipped[correspondingIndex] != "Default Pendant") { Info.Pendants.transform.Find(Info.equipped[correspondingIndex]).gameObject.SetActive(false); }
                    Info.Pendants.transform.Find(selectedItem).gameObject.SetActive(true);
                }


                HTTP.AskServer(new Dictionary<string, string> { {"request",  "equip" },
                    {"equipment type",  options[view]}, {"equipment name", selectedItem} });
                Info.equipped[correspondingIndex] = selectedItem;
                upHandler.UpdateEquip();
            }
        }
    }

    public Equipment(GameObject camera)
    {
        main = camera.transform.Find("Activated UI").transform.Find("Display").transform.Find("Equipment");
        Arrows = main.Find("Arrows").gameObject;
        Equipments = main.Find("Equipments").gameObject;
        Title = main.Find("Title").gameObject.GetComponent<TMP_Text>();
        Template = main.Find("Fake Equip").Find("Template Image").gameObject;
        weaponSpecs = main.Find("Weapon Display").gameObject;
        clothingSpecs = main.Find("Clothing Display").gameObject;
        upHandler = camera.transform.parent.GetComponent<UpdatePlayer>();
        Debug.Log("ready for activate");
}
}
