using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill  // ability spells like leap, mega jump, etc. 
{
    // info for constructor
    public static Dictionary<string, Dictionary<string, string>> SkillInfo = GameObject.Find("Game Manager").GetComponent<Game>().skills;

    // general skill variables 
    public readonly string name;
    public readonly int manaCost; 
    public readonly int staminaCost;
    public readonly float duration; // for attack, its extra cooldown for attack - for magic, its the amount of time before the magic dies off - 

    // if they need to charge up in order to do the special skill
    public readonly float chargeNeeded;
    public readonly Condition[] effects;

    public Skill(string name) // sets up all the readonly variable
    {

    }
}

public class Magic : Skill // magic spells like trap, heal, etc. 
{
    public readonly int baseDamage;
    public readonly float chargeMultiplier; // base * (1 + chargeMultiplier*charge)
    public readonly Condition[] InflictedEffects; // these are the effects that affect the caster of a magic spell

    public Magic(string name) : base(name) // sets up the two variables for magic and the base(name) shoots the super's constructor 
    {
        
    }
}


public class Attack : Skill // attack skills like two sword strike, etc. 
{
    public readonly int bonusDamage;
    public readonly float attackMultiplier;

    public Attack(string name) : base(name)
    {

    }
}
