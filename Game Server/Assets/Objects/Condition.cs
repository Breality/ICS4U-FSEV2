using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition // this item is slapped onto a player and takes effect every turn or does something at ths start
{
    public readonly string name;
    public readonly Player caster;
    public readonly Player affected; 

    public Condition(Player affected, Player caster, string name)
    {
        // set normal variables 
        this.name = name;
        this.affected = affected;
        this.caster = caster; 

        // get all info based on name and load it in

        // take primary effect

    }
    
    public bool UpdateOne() // deal any lasting effects and returns true if the condition is no longer effective 
    {
        // these are straight up if statements of similar names lumped together and effects taking place 
        return false;
    }
}
