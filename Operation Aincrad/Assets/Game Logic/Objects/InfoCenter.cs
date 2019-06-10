using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoCenter : MonoBehaviour // local data held to make life easier, changing here does not change server info
{
    // non info items
    public TextMesh MoneyText;
    public TMP_Text NameText;


    // basic info
    public string username;
    public int gold = 0;
    public int score = 0;

    // equipment in {name : count}
    public Dictionary<string, int> clothing = new Dictionary<string, int> { };
    public Dictionary<string, int>  weapons = new Dictionary<string, int> { };
    public Dictionary<string, int>  items = new Dictionary<string, int> { };
    public string[] equipped = new string[6] { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Default Sword", "None" };

    // titles
    public List<string> titles = new List<string> { };
    public string chosenTitle = "Rookie";

    // skills that the player is allowed to use
    public List<string> magicSpells = new List<string> { };
    public List<string> attackSkills = new List<string> { };
    public List<string> playerAbilities = new List<string> { };

    // quests
    public int questProg = 0; // main storyline
    public Dictionary<string, int> optionalQuests = new Dictionary<string, int> { };

    // game info
    Dictionary<string, Vector3> position = new Dictionary<string, Vector3> { }; // we'll burn that bridge when we get there

    // game stats; updated from server when needed to

    // Update is called once per frame
    int lastGold = -1;
    void Update() // checks if money has changed
    {
        if (lastGold != gold)
        {
            lastGold = gold;
            MoneyText.text = "$"+gold.ToString();
        }
    }

    public void Recalculate() // gets game stats from server
    {

    }

    public void LogIn(DBPlayer player)
    {
        // main data
        username = player.username;
        NameText.text = username;

        gold = player.gold;
        score = player.score;

        // Equipment
        equipped = player.equipped;
        foreach (string name in player.clothing)
        { // Clothing
            if (clothing.ContainsKey(name)) { clothing[name]++; }
            else { clothing[name] = 1; }
        }

        foreach (string name in player.weapons)
        { // Weapons
            if (weapons.ContainsKey(name)) { weapons[name]++; }
            else { weapons[name] = 1; }
        }

        foreach (string name in player.items)
        { // Items
            if (items.ContainsKey(name)) { items[name]++; }
            else { items[name] = 1; }
        }

        // titles
        chosenTitle = player.chosenTitle;
        titles = new List<string>(player.titles);

        // quests
        questProg = player.questProg;
        for (int i = 0; i < player.optionalQuests.Length; i++) { optionalQuests[player.optionalQuests[i]] = player.optionalProg[i]; }

        // skills
        magicSpells = new List<string>(player.magicSpells);
        attackSkills = new List<string>(player.attackSkills);
        playerAbilities = new List<string>(player.playerAbilities);
    }
}
