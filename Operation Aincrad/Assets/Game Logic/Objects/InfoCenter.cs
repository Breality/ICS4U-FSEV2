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
    public GameObject Helmets;
    public GameObject Pendants;
    public GameObject BootsLeft;
    public GameObject BootsRight;
    public GameObject Armour;

    // universal info
    public Dictionary<string, int> goldShop = new Dictionary<string, int> { // the cost of everything
        {"Ancient Sword", 50 }, {"Orochimaru", 25 }, {"Ogre Sword", 10 }, {"Silver Rapier", 40 }, {"Sword of the Abyss", 50 }, {"Templar Sword", 50 },
        { "Curved Sword", 75 }, {"Dark Sword", 75 }, {"Dothraki Sword", 50 }, {"Elucidator", 100 }, {"Long Sword", 150 }, {"Shark Staff", 200 },
        { "High Boots"  , 125}, {"White Shoes",30 }, {"Converse"      , 45 }, {"Army Boots", 55  }, {"Mining Boots",20 }, {"Military Shoes", 60 },
        { "Aviator's Hat",30 },{"Flower Hat" , 35  },
        { "Loki Helmet" , 100}, {"Soldier Helmet", 20 }, {"Sorcerer Helmet", 60 }, {"Spartan Helmet", 45 }, {"Templar Helmet", 45 },
        { "Top Hat" , 35 }, {"Viking Helmet", 45} , {"Knight Armour", 50 }, {"Aqua Dress", 40}, {"Black Dress", 45},
        {"Purple Dress", 35}, {"Red Dress", 60}, {"White Dress", 65}, {"Red Jacket", 25},
        {"Sapphire Pendant", 100 }, {"Egyptian Necklace", 40}, {"Millenium Pendant", 35}, {"Angel Pendant", 20}, {"Pirate Pendant", 15},
        { "Panther Necklace", 20}
    };

    // basic info
    public string username;
    public int gold = 0;
    public int score = 0;

    // calculated stats
    public int maxHp = 100;
    public int maxMana = 100;
    public int maxStamina = 100;

    private int mobility = 20;
    private float resist = 0;

    private float[] attackPower = new float[] { 0, 1 }; // {bonus, multiplier}
    private float[] magicPower = new float[] { 0, 1 }; // {bonus, multiplier}

    // updated stats (every .25 seconds)
    public int Hp = 100;
    public int Mana = 100;
    public int Stamina = 100;

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
    public string[] equipped = new string[6] { "Default Helmet", "Default Armour", "Default Boots", "Default Pendant", "None", "Rusty Sword" };

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

    public void ReCalculate()
    {
        int baseHp = 100;
        int baseMana = 100;
        int baseStamina = 100;
        float baseResist = 0f;
        float[] baseAttack = new float[] { 0, 1 }; // {bonus, multiplier}
        float[] baseMagic = new float[] { 0, 1 }; // {bonus, multiplier}
        int baseSpeed = 10;

        for (int i = 0; i < 4; i++)
        {
            string owned = equipped[i];
            if (owned != "None")
            {
                string index = new string[] { "Helmets", "Armour", "Boots", "Pendants" }[i];
                Clothing item = clothing[index][owned];

                baseHp += item.bonusHp;
                baseMana += item.bonusMana;
                baseStamina += item.bonusStamina;
                baseResist += item.resist;
                baseSpeed += item.bonusSpeed;

                baseAttack[0] += item.attackPower[0];
                baseAttack[1] += item.attackPower[1];

                baseMagic[0] += item.magicPower[0];
                baseMagic[1] += item.magicPower[1];
            }
        }

        maxHp = baseHp;
        maxMana = baseMana;
        maxStamina = baseStamina;
        this.resist = baseResist;
        this.mobility = baseSpeed;
        this.attackPower = baseAttack;
        this.magicPower = baseMagic;
        Debug.Log(maxHp + " " + maxMana + " " + maxStamina);
    }

    // ----------- Loading data --------------------
    private void Load(DBPlayer player)
    {
        NameText.text = username;

        gold = player.gold;
        score = player.score;

        // Equipment
        foreach (string name in player.clothing)
        {  // Clothing
            Clothing item = new Clothing(name, equipmentConstructor["Clothing"][name]);
            clothing[item.clothingType][item.name] = item;
        }

        foreach (string name in player.weapons)
        { // Weapons
            Weapon item = new Weapon(name, equipmentConstructor["Weapons"][name]);
            weapons[item.name] = item;
        }

        foreach (string name in player.items)
        { // Items
            Item item = new Item(name, equipmentConstructor["Items"][name]);
            items[item.name] = item;
        }

        // putting on their correct equipment
        equipped = player.equipped;
        if (equipped[4] != "None") { WeaponsL.transform.Find(equipped[4]).gameObject.SetActive(true); }
        if (equipped[5] != "None") { WeaponsR.transform.Find(equipped[5]).gameObject.SetActive(true); }

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

        // calculate stats again
        ReCalculate();
    }

    public string LogIn(DBPlayer player, Dictionary<string, Dictionary<string, string>> equipments) // logging in
    {
        // equipment info from the server's text file
        equipmentConstructor = equipments;
        username = player.username;

        Debug.Log(NameText);
        Load(player);

        return username;
    }

    public void NewStats(DBPlayer player) // updating player data
    {
        Load(player);
    }

    public Image HpFill;
    public Image ManaFill;
    public Image StaminaFill;
    public TMP_Text HpText;

    public void ReDraw() 
    {
        // update gold, hp, mana, stamina
        Debug.Log("Redrawn");
        Debug.Log(MoneyText);
        MoneyText.text = "$" + gold.ToString();
        HpFill.fillAmount = (float)Hp/maxHp;
        ManaFill.fillAmount = (float)Mana / maxMana;
        StaminaFill.fillAmount = (float)Stamina / maxStamina;
        HpText.text = Hp + " / " + maxHp;
    }
}
