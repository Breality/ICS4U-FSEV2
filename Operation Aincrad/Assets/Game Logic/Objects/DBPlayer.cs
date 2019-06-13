using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DBPlayer
{
    // basic info
    public string username;
    public string hash;

    // equipment info
    public string[] clothing;
    public string[] weapons;
    public string[] items;
    public string[] equipped = new string[6] { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Default Sword", "None" };

    // stat info
    public int gold = 0;
    public int score = 0;
    public string[] titles;
    public string chosenTitle = "Rookie";

    // quest info
    public int questProg = 0; // main storyline
    public string[] optionalQuests;
    public int[] optionalProg;

    // special abilities they have learned
    public string[] magicSpells;
    public string[] attackSkills;
    public string[] playerAbilities;

    // constructor for xml (is never used)
    public DBPlayer()
    {
        username = "not set";
        hash = "not set";

        clothing = new string[] { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant" };
        weapons = new string[] { "Default Sword" };
        items = new string[] { "Red Potion", "Red Potion", "Red Potion", "Blue Potion" };

        titles = new string[] { "Rookie", "Newbie" };
        optionalQuests = new string[] { "Hidden" };
        optionalProg = new int[] { 0 };

        magicSpells = new string[] { "Default Teleport" };
        attackSkills = new string[] { "Double Slash" };
        playerAbilities = new string[] { "Sprint" };
    }

}
