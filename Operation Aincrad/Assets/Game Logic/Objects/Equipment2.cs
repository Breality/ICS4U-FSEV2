using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ICS4U-01
 * Mr. McKenzie
 * Anish Aggarwal, Noor Nasri, Zhehai Zhang
 * June 14th, 2019
 * Equipment2 Class
 * Description:
 * This class handles the equipment stats and effects from a text file
 */

public class Equipment2
{
    //Shouldn't be able to be edited
    public readonly string name;
    public readonly string rarity;

    //Initalize the stats for equipment
    public Equipment2(string name, string info)
    {
        this.name = name;

        string[] items = info.Split(',');
        rarity = items[0];
    }
}

public class Item : Equipment2
{
    public readonly int bonusHp;
    public readonly int bonusMana;
    public readonly int bonusStamina;

    public Item(string name, string info) : base(name, info)
    {
        // Stat for an equipment: Rarity, hp, mana, stamina, resist, magic bonus, magic multiplier, attack bonus, attack multiplier
        string[] items = info.Split(','); 

        //Set the first few stats
        bonusHp = int.Parse(items[1]);
        bonusMana = int.Parse(items[2]);
        bonusStamina = int.Parse(items[3]);
    }
}

public class Clothing : Equipment2
{
    public readonly string clothingType;
    public readonly int bonusHp;
    public readonly int bonusMana;
    public readonly int bonusStamina;

    //Other stats
    public readonly float resist;
    public readonly float[] attackPower = new float[] { 0, 1 }; // {bonus, multiplier}
    public readonly float[] magicPower = new float[] { 0, 1 }; // {bonus, multiplier}

    public readonly int bonusSpeed;

    //Parses text that describes all the stats
    public Clothing(string name, string info) : base(name, info)
    {
        string[] items = info.Split(','); // Rarity, hp, mana, stamina, resist, attack Bonus, attack multiplier, magic bonus, magic multiplier, speed
        clothingType = items[1];
        bonusHp = int.Parse(items[2]);
        bonusMana = int.Parse(items[3]);
        bonusStamina = int.Parse(items[4]);
        resist = float.Parse(items[5]);
        attackPower = new float[] { float.Parse(items[6]), float.Parse(items[7]) };
        magicPower = new float[] { float.Parse(items[8]), float.Parse(items[9]) };
        bonusSpeed = int.Parse(items[10]);
    }
}

//Weapons have different effects than the other equipment
public class Weapon : Equipment2
{
    // Weapon variables that are specific to the weapon name
    public readonly double attack;
    public readonly double pierce;
    public readonly double range;

    public readonly int mana;
    public readonly int stamina;
    public readonly int weaponType; // 0 = left handed, 1 = right handed, 2 = both

    public readonly float cooldown; // number of seconds between the end of an attack and the allowed start of the next attack
    public readonly float chargeEffect; // this is a multiplier for default damage. chargeEffect*charge*attack + attack
    // public readonly Dictionary<string, Attack> skills = new Dictionary<string, Attack> { };
    private readonly float attackLength = 1; // amount of seconds before the attack ends

    // private variables used for keeping track of the weapon
    private float attackEnd = -1;

    // getters
    public bool isAttacking() { return (attackEnd > Time.time); }

    // Weapon constructor, all stored as a string from a text file, loaded in when needed to
    public Weapon(string name, string info) : base(name, info)
    {
        string[] items = info.Split(','); //  Rarity, attack, pierce, range, mana, stamina, type (0 = left handed, 1 = right handed, 2 = both), attack cooldown, charge bonus, attack duration, every skill it can use seperated by comma

        attack = double.Parse(items[1]);
        pierce = double.Parse(items[2]);
        range = double.Parse(items[3]);
        mana = int.Parse(items[4]);
        stamina = int.Parse(items[5]);
        weaponType = int.Parse(items[6]);
        cooldown = float.Parse(items[7]);
        chargeEffect = float.Parse(items[8]);
        attackLength = float.Parse(items[9]);
        
    }
    
}

