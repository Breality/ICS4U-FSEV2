﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill  // ability spells like leap, mega jump, etc. 
{
    // general skill variables 
    public readonly string name;
    public readonly int manaCost; 
    public readonly int staminaCost;
    public readonly float duration;

    // if they need to charge up in order to do the special skill
    public readonly float chargeNeeded;
    public readonly Condition[] effects; 
}

public class Magic : Skill // magic spells like trap, heal, etc. 
{
    public readonly int baseDamage;
    public readonly float chargeMultiplier; // base * (1 + chargeMultiplier*charge)
}


public class Attack : Skill // attack skills like two sword strike, etc. 
{
    public readonly int bonusDamage; // not a multiplier, its fixed added damage
    
}
