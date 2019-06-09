using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DBPlayer  // ability spells like leap, mega jump, etc. 
{
    public string username;
    //public string hash;
    //public string salt;
    public string password;
    public int score;

    public DBPlayer(string user, string pass, int score)
    {
        username = user;
        password = pass;
        this.score = score;

    }
}
