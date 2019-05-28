using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    // general skill variables 
    public readonly string name;
    public readonly int manaCost; 
    public readonly int staminaCost;
    public readonly float duration;

    // if they need to charge up in order to do the special skill
    public readonly float chargeNeeded; 
}

public class Magic : Skill // magic spells like trap, heal, etc. 
{

}

public class Ability : Skill // ability spells like leap, mega jump, etc. 
{

}

public class Attack : Skill // attack skills like two sword strike, etc. 
{
    public readonly float bonusDamage;
    
}
