using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public readonly Player owner;
    public readonly string name;
    private int quantity; 

    public int HowMany() { return quantity; }
}

public class Clothing : Equipment
{
    
}

public class Weapon : Equipment
{
    // Weapon variables that are specific to the weapon name
    public readonly double attack;
    public readonly double pierce; 
    public readonly double range;

    public readonly int mana;
    public readonly int stamina;
    public readonly int weaponType; // 0 = left handed, 1 = right handed, 2 = both

    public readonly float cooldown;
    public readonly float chargeEffect; // this is a multiplier for default damage. chargeEffect*charge*attack + attack
    public readonly Dictionary<string, Attack> skills;
    private readonly float attackLength = 1; // amount of seconds before the attack ends

    // private variables used for keeping track of the weapon
    private float attackEnd = -1;
    
    // getters
    public bool isAttacking() { return (attackEnd > Time.time); }

    // Weapon constructor, all stored as a string from a text file, loaded in when needed to
    public Weapon(string constructor)
    {
        
    }

    // variables dictated when they decide on something 
    private int slashAttack = -1;
    private Condition[] bonusEffect = null;

    // activates the attack, gets it all ready for when the hit function is called. returns true if attack was activated, false if something is wrong
    public bool Attack(Attack attackSkill, float charge) 
    {
        // get variables specific for this attack (changes with attack skill)
        if (Time.time >= attackEnd + cooldown)
        {
            int staminaCost = (attackSkill == null) ? stamina : stamina + attackSkill.staminaCost;
            int manaCost = (attackSkill == null) ? mana : mana + attackSkill.manaCost;

            if (!(attackSkill != null && charge < attackSkill.chargeNeeded) && owner.Cost(staminaCost, manaCost)) // Charge, stamina and mana needed
            {
                attackEnd = (attackSkill == null) ? Time.time + attackLength : Time.time + attackLength + attackSkill.duration;

                slashAttack = (int)(attack * (charge * chargeEffect + 1));
                bonusEffect = null;

                // set up attack damage and buffs/debuffs
                return true;
            }
        }

        return false;
    }
    
    public int getSlash() { return slashAttack; }

    public Condition[] getEffects(Player hit) {
        Condition[] newConditions = new Condition[bonusEffect.Length];
        for (int i = 0; i < bonusEffect.Length; i++)
        {
            Condition c = bonusEffect[i];
            Condition newC = new Condition(hit, owner, c.name);
            newConditions[i] = newC; 
        }
        return newConditions;
    }
}

public class Item : Equipment
{

}
