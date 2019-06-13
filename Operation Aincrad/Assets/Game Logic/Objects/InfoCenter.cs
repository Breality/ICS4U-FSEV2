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
    public GameObject WeaponsL;
    public GameObject WeaponsR;

    // universal info
    public Dictionary<string, int> goldShop = new Dictionary<string, int> { // the cost of everything
        {"Ancient Sword", 50 }, {"Orochimaru", 25 }, {"Ogre Sword", 10 }, {"Silver Rapier", 40 }, {"Sword of the Abyss", 50 }, {"Templar Sword", 50 },
        { "Curved Sword", 75 }, {"Dark Sword", 75 }, {"Dothraki Sword", 50 }, {"Elucidator", 100 }, {"Long Sword", 150 }, {"Shark Staff", 200 }
    };

    // basic info
    public string username;
    public int gold = 0;
    public int score = 0;

    // equipment constructor
    public Dictionary<string, Dictionary<string, string>> equipmentConstructor;

    // constructed equipments that are owned
    public Dictionary<string, Dictionary<string, Clothing>> clothing = new Dictionary<string, Dictionary<string, Clothing>> {
        {"Helmets", new Dictionary<string, Clothing>{ } },
        {"Armour", new Dictionary<string, Clothing> { } },
        {"Boots", new Dictionary<string, Clothing> { } },
        {"Pendants", new Dictionary<string, Clothing>{ } },
    };
    public Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon> { };
    public Dictionary<string, Item> items = new Dictionary<string, Item> { };
    
    // items owned that are currently equiped
    public string[] equipped = new string[6] { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "Rusty Sword", "None" };

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

    public void LogIn(DBPlayer player, Dictionary<string, Dictionary<string, string>> equipments)
    {
        // equipment info from the server's text file
        equipmentConstructor = equipments;

        // main data
        username = player.username;
        NameText.text = username;

        gold = player.gold;
        score = player.score;

        // Equipment
        foreach (string name in player.clothing)
        {  // Clothing
            Clothing item = new Clothing(name, equipments["Clothing"][name]);
            clothing[item.clothingType][item.name] = item;
        }

        foreach (string name in player.weapons)
        { // Weapons
            Weapon item = new Weapon(name, equipments["Weapons"][name]);
            weapons[item.name] = item;
        }

        foreach (string name in player.items)
        { // Items
            Item item = new Item(name, equipments["Items"][name]);
            items[item.name] = item;
        }
        // putting on their correct equipment
        equipped = player.equipped;
        if (equipped[4] != null)  { WeaponsR.transform.Find(equipped[4]).gameObject.SetActive(true); }
        if (equipped[5] != null) { WeaponsR.transform.Find(equipped[4]).gameObject.SetActive(false); }

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
