using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition // this item is slapped onto a player and takes effect every turn or does something at ths start
{
    public readonly string name;
    public readonly Player caster;
    public readonly Player affected;
    public readonly float lifespan;

    public readonly string type; // the type of condition
    public readonly bool onActive; // something other than stat boost
    public readonly float infoNeeded; // any special info we need

    // stat boosts that activate when the condition is used, set from the text file 
    public readonly Dictionary<string, float> statChanges = new Dictionary<string, float> { {"Speed", 0}, {"Hp Regen", 0}, {"Mana Regen", 0}, {"Stamina Regen", 0}, {"Damage Multiplier", 0} };
    
    public readonly float timeStarted = Time.time;

    public Condition(Player affected, Player caster, string name)
    {
        // set normal variables 
        this.name = name;
        this.affected = affected;
        this.caster = caster; 

        // get all info based on name and load it in
        if (onActive)
        {
            if (type.Equals("Remove Conditions")) 
            {

            }else if (type.Equals(""))
            {

            }
        }
    }
    
    public bool UpdateOne() // deal any lasting effects and returns true if the condition is no longer effective 
    {
        // check if it needs to update again or not
        if (Time.time > lifespan + timeStarted)
        {
            return true;
        }

        // if statements of similar names lumped together and effects taking place 

        return false;
    }
}
