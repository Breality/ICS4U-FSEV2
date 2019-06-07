using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{
    public static readonly Dictionary<string, string> equipmentInfo = new Dictionary<string, string> { }; // for constructing equipment {name : html}

    public readonly Player owner;
    public readonly string name;
    public readonly string rarity;

    protected int quantity; 

    public int HowMany() { return quantity; }
    public void NewCount(int number)
    {
        quantity = number;
    }

    public Equipment(Player owner, string name, string info)
    {
        this.owner = owner;
        this.name = name;

        string[] items = info.Split(',');
        rarity = items[0];
    }
}

public class Item : Equipment
{
    public readonly int bonusHp;
    public readonly int bonusMana;
    public readonly int bonusStamina;
    public readonly string usageType; // "Cast on self", "Field effect", "Castable on other players"
    public readonly Condition[] effects; // temp speed, long term heal, etc.

    public Item(Player owner, string name, string info) : base(owner, name, info)
    { // Rarity, hp, mana, stamina, use type, every condition it activates when used on a player

    }
}

public class Clothing : Equipment
{
    public readonly int bonusHp;
    public readonly int bonusMana;
    public readonly int bonusStamina;

    public readonly float resist;
    public readonly float[] attackPower = new float[] { 0, 1 }; // {bonus, multiplier}
    public readonly float[]  magicPower = new float[] { 0, 1 }; // {bonus, multiplier}

    public readonly int bonusSpeed;

    public Clothing(Player owner, string name, string info) : base(owner, name, info)
    {
        string[] items = info.Split(','); // Rarity, hp, mana, stamina, resist, attack Bonus, attack multiplier, magic bonus, magic multiplier, speed

        bonusHp = int.Parse(items[1]);
        bonusMana = int.Parse(items[2]);
        bonusStamina = int.Parse(items[3]);
        resist = float.Parse(items[4]);
        attackPower = new float[] { float.Parse(items[5]), float.Parse(items[6]) } ;
        magicPower = new float[] { float.Parse(items[7]), float.Parse(items[8]) };
        bonusSpeed = int.Parse(items[9]);
    }
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

    public readonly float cooldown; // number of seconds between the end of an attack and the allowed start of the next attack
    public readonly float chargeEffect; // this is a multiplier for default damage. chargeEffect*charge*attack + attack
    public readonly Dictionary<string, Attack> skills = new Dictionary<string, Attack> { };
    private readonly float attackLength = 1; // amount of seconds before the attack ends

    // private variables used for keeping track of the weapon
    private float attackEnd = -1;
    
    // getters
    public bool isAttacking() { return (attackEnd > Time.time); }

    // Weapon constructor, all stored as a string from a text file, loaded in when needed to
    public Weapon(Player owner, string name, string info) : base(owner, name, info)
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

        for (int i=10; i<items.Length-1; i++) // loop through all the skill names and add them
        {
            skills[items[i]] = new Attack(items[i]);
        }
    }

    // variables dictated when they decide on attacking 
    private int slashAttack = -1;
    private List<Condition> bonusEffect = new List<Condition> { };

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
                bonusEffect = new List<Condition> { };

                if (attackSkill != null)
                {
                    foreach (Condition c in attackSkill.effects)
                    {
                        bonusEffect.Add(c);
                    }
                    slashAttack += attackSkill.bonusDamage;
                    slashAttack = (int) (slashAttack*attackSkill.attackMultiplier);
                }
                
                return true;
            }
        }

        return false;
    }
    
    public int getSlash() { return slashAttack; }

    public Condition[] getEffects(Player hit) {
        Condition[] newConditions = new Condition[bonusEffect.Count];
        for (int i = 0; i < bonusEffect.Count; i++)
        {
            Condition c = bonusEffect[i];
            Condition newC = new Condition(hit, owner, c.name);
            newConditions[i] = newC; 
        }
        return newConditions;
    }
}

