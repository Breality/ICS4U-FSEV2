using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition // this item is slapped onto a player and takes effect every turn or does something at ths start
{
    public readonly string name;
    public readonly Player user;
    public readonly Player affected; 

    public Condition(Player activeOn, Player caster, string name)
    {
        // set normal variables 
        this.name = name;
        affected = activeOn;
        user = caster; 

        // get all info based on name and load it in

        // take primary effect
    }

    public void UpdateOne() // deal continous or detect when its over
    {

    }
}
