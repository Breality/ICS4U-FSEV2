using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DBPlayer 
{
    public string username;
    public string hash;
    public string info;

    public DBPlayer(string username, string hash, string info)
    {
        this.username = username;
        this.hash = hash;
        this.info = info ?? "default";
    }
}
