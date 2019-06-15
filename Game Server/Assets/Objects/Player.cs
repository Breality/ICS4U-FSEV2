using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    // -------------- Class variables  --------------
    public static readonly Dictionary<string, double> staminaCosts = new Dictionary<string, double> { { "Idle", 0 }, { "Walking", 1 }, { "Sprinting", 3 } };
    public static Game game = GameObject.Find("Game Manager").GetComponent<Game>();

    // Identifiers < Store on login
    public readonly string Username;
    private readonly string Password;
    private readonly string Token;

    // Player's inventory < Save and Load. Clothing, Weapon, and Item all inherit from Equipment.
    private string[] equipped = new string[6]; // 0 = Helmet, 1 = Body Armour, 2 = Boots, 3 = Pendulum, 4 = Weapon 1, 5 = Weapon 2 
    private Dictionary<string, Clothing> clothing = new Dictionary<string, Clothing> { };
    private Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon> { };
    private Dictionary<string, Item> items = new Dictionary<string, Item> { };

    // Game stats < Save and Load
    private int gold = 0;
    private List<string> titles = new List<string> { };
    private string chosenTitle = "Player";
    private int score = 0; // for world leaderboard

    // Quest stats < Save and Load
    private int mainProgress = 0; // the progress in the main storyline
    private Dictionary<string, int> optionalQuests = new Dictionary<string, int> { }; // the quests they have taken and their progress on them. -1 for completed

    // Special Skills < Save and Load. Magic, Attack and Ability all inherit from Skill
    private Dictionary<string, Magic> magicSpells = new Dictionary<string, Magic> { };
    private Dictionary<string, Attack> attackSkills = new Dictionary<string, Attack> { };
    private Dictionary<string, Skill> playerAbilities = new Dictionary<string, Skill> { };

    // Combat stats < Recalculate when needed
    private int[] hp = new int[] { 100, 100 }; // {max, current}
    private int[] mana = new int[] { 100, 100 }; // {max, current}
    private int[] stamina = new int[] { 100, 100 }; // {max, current}

    private float resist = 0;
    private float[] attackPower = new float[] { 0, 1 }; // {bonus, multiplier}
    private float[] magicPower = new float[] { 0, 1 }; // {bonus, multiplier}

    private int mobility = 20;

    // Game variables for playing
    private string status = "idle";
    private Vector3 position = new Vector3(); // add starting position
    private int guildID = -1;
    private float charge = -1; // the time they started charging. When the charge is used, it is reset to -1

    private List<Condition> currentConditions = new List<Condition> { }; // poison, heal, slow, boost, etc. 
    Dictionary<string, float> conditionStats = new Dictionary<string, float> { { "Speed", 0 }, { "Hp Regen", 0 }, { "Mana Regen", 0 }, { "Stamina Regen", 0 }, { "Damage Multiplier", 0 } };


    // -------------- Public getters --------------
    public int GetGuild() { return guildID; }
    public float GetCharge() { return Time.time - charge; }
    public Vector3 GetPos(){ return position; }
    public int[] GetStats() { return new int[] { hp[1], mana[1], stamina[1] }; }

    public int GetGold() { return gold; }
    public int GetScore() { return score; }
    public string GetTitle() { return chosenTitle; }
    public int GetProg() { return mainProgress; }
    public string[] GetEquipped() { return (string[])equipped.Clone(); }

    // -------------- Save getters --------------
    public string[] GetClothing() // converting the private clothing to the format we store it in DBPlayer
    {
        List<string> items = new List<string> { };
        foreach (KeyValuePair<string, Clothing> item in clothing) { for (int i=0; i < item.Value.HowMany(); i++) { items.Add(item.Key); } }
        return items.ToArray();
    }

    public string[] GetWeapons() // converting the private weapons to the format we store it in DBPlayer
    {
        List<string> items = new List<string> { };
        foreach (KeyValuePair<string, Weapon> item in weapons) {for (int i = 0; i < item.Value.HowMany(); i++) { items.Add(item.Key); } }

        return items.ToArray();
    }

    public string[] GetItems() // converting the private items to the format we store it in DBPlayer
    {
        List<string> items = new List<string> { };
        foreach (KeyValuePair<string, Item> item in this.items) { for (int i = 0; i < item.Value.HowMany(); i++) { items.Add(item.Key); } }
        return items.ToArray();
    }

    public string[] GetTitles() { return titles.ToArray(); }
    public Dictionary<string, int> GetQuests() { return optionalQuests; } // sent as it is to be split into string[] and int[]

    public string[] GetMagic() { return magicSpells.Keys.ToArray(); }
    public string[] GetAttacks() { return attackSkills.Keys.ToArray(); }
    public string[] GetAbilities() { return playerAbilities.Keys.ToArray(); }

    // -------------- Class constructor --------------
    public Player(DBPlayer player, string token) // gets the html from server, sets up player data 
    {
        Username = player.username;
        Token = token;

        // -------- load all the info into the private fields, DBPlayer loses referance and dies afterwards --------
        // Equipment
        Debug.Log("Right hand: " + player.equipped[5]);
        equipped = player.equipped;
        foreach (string name in player.clothing) { // Clothing
            if (clothing.ContainsKey(name)) { clothing[name].NewCount(clothing[name].HowMany() + 1); } 
            else { clothing[name] = new Clothing(this, name, game.equipments["Clothing"][name]); }
        }

        foreach (string name in player.weapons) { // Weapons
            if (weapons.ContainsKey(name)) { weapons[name].NewCount(weapons[name].HowMany() + 1); }
            else { weapons[name] = new Weapon(this, name, game.equipments["Weapons"][name]); }
        }

        foreach (string name in player.items) { // Items
            if (items.ContainsKey(name)) { items[name].NewCount(items[name].HowMany() + 1); }
            else { items[name] = new Item(this, name, game.equipments["Items"][name]); }
        }

        // stats
        gold = player.gold;
        score = player.score;
        chosenTitle = player.chosenTitle;
        titles = new List<string>(player.titles);

        // quests
        mainProgress = player.questProg;
        for (int i=0; i<player.optionalQuests.Length; i++) { optionalQuests[player.optionalQuests[i]] = player.optionalProg[i]; }

        // skills
        foreach (string name in player.magicSpells) { magicSpells[name] = new Magic(name); }
        foreach (string name in player.attackSkills) { attackSkills[name] = new Attack(name); }
        foreach (string name in player.playerAbilities) { playerAbilities[name] = new Skill(name); }

        ReCalculate(); // get new stats and all for player before slapping them into the game

        hp[1] = hp[0];
        mana[1] = mana[0];
        stamina[1] = stamina[0];
    }

    // -------------- These functions deal damage to the players, returns true if killed --------------
    // private overload called by all the public overloads
    private bool TakeDamage(int damage) // takes normal damage, applies any conditional bonuses 
    {
        int dam = (int)(damage * conditionStats["Damage Multiplier"]);
        hp[1] -= dam;
        Debug.Log(Username + " just took " + dam + " damage and has " + hp[1] + " health left");
        if (hp[1] <= 0)
        {
            return true;
        }
        return false;
    }

    // This is overloaded for a weapon attack
    private Dictionary<Weapon, float> lastHit = new Dictionary<Weapon, float> { };
    public bool TakeDamage(Weapon weapon) 
    {
        if (weapon.isAttacking() && (!lastHit.ContainsKey(weapon) || Time.time - lastHit[weapon] > 0.2f))
        {
            lastHit[weapon] = Time.time;
            foreach (Condition cond in weapon.getEffects(this))
            {
                currentConditions.Add(cond); // put a copy of the condition in (the function gets us newly made copies)
            }

            return TakeDamage(weapon.getSlash());
        }
        return false;
    }

    // This is overloaded for a magic spell
    public bool TakeDamage(Magic spell)
    {
        return false; 
    }

    // This is overloaded for a curse/heal effect 
    public bool TakeDamage(Condition cond) { return TakeDamage(cond.damage); }
    // deals with health regen, poison effects, whatever
    public void UpdateOne() 
    {
        hp[1] = Mathf.Min(hp[0], hp[1] + hp[0] / 100);
        mana[1] = Mathf.Min(mana[0], mana[1] + mana[0] / 100);
        stamina[1] = Mathf.Min(stamina[0], stamina[1] + stamina[0] / 100);

        foreach (Condition effect in currentConditions)
        {
            effect.UpdateOne();
        }
    }

    // -------------- These methods are called by other classes that don't have acess to things --------------
    public void removeConditions(bool RemoveKind)
    {
        List<Condition> newConditions = new List<Condition> { };

        foreach (Condition c in currentConditions)
        {
            if (c.isHelpful != RemoveKind)
            {
                newConditions.Add(c);
            }
        }

        currentConditions = newConditions;
    }

    public void updateConditions() // goes through the conditions and reconstructs the stat changes from the conditions placed on you
    {
        Dictionary<string, float>  temp = new Dictionary<string, float> { { "Speed", 0 }, { "Hp Regen", 0 }, { "Mana Regen", 0 }, { "Stamina Regen", 0 }, { "Damage Multiplier", 0 } };

        foreach (Condition c in currentConditions)
        {
            foreach (KeyValuePair<string, float> stat in c.statChanges)
            {
                temp[stat.Key] += stat.Value;
            }
        }

        conditionStats = temp;
    }

    public Weapon CheckWeapon(string name) // returns the weapon in question for when a player gets hit
    {
        if (equipped[4] == name || equipped[5] == name) // if it is equipped
        {
            return weapons[name];
        }
        return null;
    }

    public bool Cost(int stamina=0, int mana=0, int gold=0)
    {
        if (this.mana[1] >= mana && this.stamina[1] >= stamina && this.gold >= gold)
        {
            this.mana[1] -= mana;
            this.stamina[1] -= stamina;
            this.gold -= gold;
            return true;
        }
        return false;
    }

    public void Charge(int status)
    {
        charge = (status == 1) ? Time.time : -1;
    }

    public void ReCalculate()
    {
        int baseHp = 100;
        int baseMana = 100;
        int baseStamina = 100;
        float baseResist = 0f;
        float[] baseAttack = new float[] { 0, 1 }; // {bonus, multiplier}
        float[] baseMagic = new float[] { 0, 1 }; // {bonus, multiplier}
        int baseSpeed = 10;
        
        for (int i=0; i < 4; i++)
        {
            string owned = equipped[i];
            if (owned != "None")
            {
                Clothing item = clothing[owned];

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

        this.hp[0] = baseHp;
        this.mana[0] = baseMana;
        this.stamina[0] = baseStamina;
        this.resist = baseResist;
        this.mobility = baseSpeed;
        this.attackPower = baseAttack;
        this.magicPower = baseMagic;
    }

    public void ChangeEquipped(int index, string newItem)
    {
        equipped[index] = newItem;
    }

    public bool Purchase(string itemType, string item, int cost)
    {
        if (cost >= gold)
        {
            gold -= cost;

            if (itemType.Equals("weapon"))
            {
                if (weapons.ContainsKey(item)) // bought the same weapon again
                {
                    weapons[item].NewCount(weapons[item].HowMany() + 1);
                }
                else
                {
                    weapons[item] = new Weapon(this, item, game.equipments["Weapons"][item]);
                }
            }
            else
            {
                if (weapons.ContainsKey(item)) // bought the same weapon again
                {
                    weapons[item].NewCount(weapons[item].HowMany() + 1);
                }
                else
                {
                    weapons[item] = new Weapon(this, item, game.equipments["Weapons"][item]);
                }
            }
            
            return true;
        }
        return false;
    }

    

    public void Kill(Player poorLad) // we killed someone, lets get some gold
    {
        int moneyMade = poorLad.Killed();
        if (moneyMade == -1) // player wasn't killed, this function was called by an exploit 
        {
            return;
        }

        gold += Math.Max(10, moneyMade); // get at least 10 gold for killing someone
    }

    private int Killed() // we got killed, 
    {
        if (hp[1] > 0)
        {
            return -1; // no, we are not dead.
        }

        int chunkLost = Math.Min((int) (gold * 0.1), 150); // lose 10% of your gold but not more than 150
        gold -= chunkLost;
        return chunkLost;
    }
}