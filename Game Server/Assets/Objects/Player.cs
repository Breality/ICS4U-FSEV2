using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    // -------------- Class variables  --------------
    public static readonly Dictionary<string, double> staminaCosts = new Dictionary<string, double> { { "Idle", 0 }, { "Walking", 1 }, { "Sprinting", 3 } };

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
    private Vector3 position = new Vector3();
    private int guildID = -1;
    private float charge = -1; // the time they started charging. When the charge is used, it is reset to -1

    private List<Condition> currentConditions = new List<Condition> { }; // poison, heal, slow, boost, etc. 
    Dictionary<string, float> conditionStats = new Dictionary<string, float> { { "Speed", 0 }, { "Hp Regen", 0 }, { "Mana Regen", 0 }, { "Stamina Regen", 0 }, { "Damage Multiplier", 0 } };


    // -------------- Public getters --------------
    public int getGuild() { return guildID; }
    public float getCharge() { return Time.time - charge; }
    public Vector3 getPos(){ return position; }
    public int[] getStats() { return new int[] { hp[1], mana[1], stamina[1] }; }

    // -------------- Class constructor --------------
    public Player(DBPlayer player, string token) // gets the html from server, sets up player data 
    {
        Username = player.username;
        Token = token;

        // -------- load all the info into the private fields, DBPlayer loses referance and dies afterwards --------
        // Equipment
        foreach (string name in player.clothing)
        {
            //clothing[name] = new Clothing(this, name, equi)
        }
        //weapons = player.weapons;
        //items = player.items;



    }

    // -------------- These functions deal damage to the players, returns true if killed --------------
    private Dictionary<Weapon, float> lastHit = new Dictionary<Weapon, float> { };

    // private overload called by all the public overloads
    private bool TakeDamage(int damage) // takes normal damage, applies any conditional bonuses 
    {
        hp[1] -= (int) (damage* conditionStats["Damage Multiplier"]);
        if (hp[1] <= 0)
        {
            return true;
        }
        return false;
    }

    // This is overloaded for a weapon attack
    public bool TakeDamage(Weapon weapon) 
    {
        if (weapon.isAttacking())
        {
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
}