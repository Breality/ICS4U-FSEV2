using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public readonly string name;
    public readonly string manaCost; 
    public readonly string staminaCost;
    public readonly float chargeNeeded; // if they need to charge up in order to do it
}

public class Magic : Skill
{

}

public class Ability : Skill
{

}

public class Attack : Skill
{
    public readonly float bonusDamage;
    
}
