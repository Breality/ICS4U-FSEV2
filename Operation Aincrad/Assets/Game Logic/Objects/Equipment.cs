﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // ------------- Functions for equipment -------------
    private void ReloadMenu()
    {
        Title.text = options[view];
        foreach (Transform child in Equipments.transform) { Object.Destroy(child.gameObject); }
        
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

            // new positions
            newItem.transform.parent = Equipments.transform;
            newItem.transform.rotation = Template.transform.rotation;
            newItem.transform.localScale = new Vector3(0.002f, 0.004f, 1);
            newItem.transform.localPosition = new Vector3(0.35f * (i % 3 - 1), (i >= 3) ? -0.35f : 0.35f, Template.transform.localPosition.z);

            // show product
            Sprite image = Resources.Load<Sprite>("Equipment Images/" + options[view] + " Dealer/" + item);
            newItem.GetComponent<Image>().sprite = image;
            newItem.SetActive(true);
            i++;
        }
    }
    

    // ------------- Functions that are called by the MenuToggle raycasting -------------
    public new List<GameObject> Activated()  // returns parents of buttons to look for
    {
        Debug.Log("Equipment Being activated");
        ReloadMenu();
        return (new List<GameObject> { Arrows, Equipments });
    }

    string selectedItem = null;
    public new void Hover(GameObject item)
    {
        // show new item and stats
        if (item.transform.parent.name == "Equipments" && view > 0) // equipment
        {
            
        } else if(item.transform.parent.name == "Equipments") // weapons
        {
            // display the new hovered item's info
            weaponSpecs.transform.Find("Selected Name").GetComponent<TMP_Text>().text = item.name;
            weaponSpecs.transform.Find("Selected Image").GetComponent<Image>().sprite = item.GetComponent<Image>().sprite;
            
            // displaying the weapon specs
            Weapon weapon = Info.weapons[item.name];
            weaponSpecs.transform.Find("Attack").GetComponent<TMP_Text>().text = "Attack: " + weapon.attack;
            weaponSpecs.transform.Find("Pierce").GetComponent<TMP_Text>().text = "Pierce: " + weapon.pierce;
            weaponSpecs.transform.Find("Range").GetComponent<TMP_Text>().text = "Range: " + weapon.range;
            weaponSpecs.transform.Find("Hand Positioning").GetComponent<TMP_Text>().text = new string[] { "Left Handed", "Right Handed", "Dual Weild" }[weapon.weaponType];
        }
    }

    public new void UnHover(GameObject item)
    {

    }

    private float lastClick = 0; // debounce system

    public new void Clicked(GameObject item)
    {
        if (Time.time - lastClick < 0.5f){ return; }
        lastClick = Time.time;

        if (item.transform.parent.name == "Arrows") // the arrows, toggling
        {
            view = (view + int.Parse(item.name) + options.Length)% options.Length;
            ReloadMenu();
        }else if (selectedItem != null) // they want to equip new item
        {
            if (view == 0) // changing weapons
            {
                Weapon newWeapon = Info.weapons[selectedItem]; // { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Rusty Sword", "None" };
                if (newWeapon.weaponType == 0) { // left hand
                    if (Info.equipped[4] != "None") { Info.WeaponsL.transform.Find(Info.equipped[4]).gameObject.SetActive(false); }
                    Info.WeaponsL.transform.Find(selectedItem).gameObject.SetActive(true);
                } 
                else if (newWeapon.weaponType == 1 && Info.equipped[5] != "None") { // right hand
                    Info.WeaponsR.transform.Find(Info.equipped[5]).gameObject.SetActive(false);
                    Info.WeaponsR.transform.Find(selectedItem).gameObject.SetActive(true);
                } 
                else if (newWeapon.weaponType == 2)
                {
                    Debug.Log("Not sure what to do here");
                }

                HTTP.AskServer(new Dictionary<string, string> { {"Request",  "Equip" },
                    {"Equipment Type",  "Weapon" }, {"Equipment Name", selectedItem} });
            }
            ReloadMenu();
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

        Debug.Log("ready for activate");
}
}
