using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public readonly string name;
    private int quantity; // how many of these items you own
}

public class Clothing : Equipment
{
    
}

public class Weapon : Equipment
{
    public readonly Player owner;
    public readonly double attack;
    public readonly double pierce; 
    public readonly double range;

    public readonly int mana;
    public readonly int stamina;

    private float lastUsed = Time.time;
    public readonly float cooldown;
    public readonly float chargeEffect; // this is a multiplier for default damage. chargeEffect*charge*attack + attack

    public readonly Dictionary<string, Attack> skills;
    

    public Weapon(string constructor)
    {
        // all stored in text file, loaded in when needed to
    }
    

    public void Attack(Player hit, Attack attackSkill, float charge) // if no attack skill, then default damage
    {
        owner
    }

}

public class Item : Equipment
{

}
