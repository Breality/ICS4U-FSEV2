// Noor's Server
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Handles game logic. Animations pass right through from client to client, position checks (not setters, but checkers) occur here and static game logic takes place. 
public class Game : MonoBehaviour
{
    // -------------- Game variables --------------
    public TextAsset equipmentFile;
    public TextAsset skillFile;
    public TextAsset conditionList;

    private Dictionary<string, Player> players = new Dictionary<string, Player> { };
    public Dictionary<Player, bool> updatePlayer = new Dictionary<Player, bool> { };

    private List<Monster> monsters = new List<Monster> { };
    private List<NPC> npcs = new List<NPC> { };

    public Dictionary<string, Dictionary<string, string>> equipments = new Dictionary<string, Dictionary<string, string>> { };
    public Dictionary<string, Dictionary<string, string>> skills = new Dictionary<string, Dictionary<string, string>> { };
    public Dictionary<string, Dictionary<string, string>> conditions = new Dictionary<string, Dictionary<string, string>> { };

    public Dictionary<string, int> goldShop = new Dictionary<string, int> {
        {"Ancient Sword", 50 }, {"Orochimaru", 25 }, {"Ogre Sword", 10 }, {"Silver Rapier", 40 }, {"Sword of the Abyss", 50 }, {"Templar Sword", 50 }, {"Curved Sword", 75 }, {"Dark Sword", 75 }, {"Dothraki Sword", 50 }, {"Elucidator", 100 }, {"Long Sword", 150 }, {"Shark Staff", 200 }
    };

    // -------------- Player decisions sent by HTTP --------------
    public void PlayerEnter(Player player, string token) { players[token] = player; updatePlayer[player] = false;  Debug.Log("Player added to server"); }

    public void PlayerLeave(string token) { players.Remove(token); }

    public void Purchase(string token, string type, string itemName)
    {
        Debug.Log("We got a purchase request: " + token + ", " + type + ", " + itemName);
        Player player = players[token];
        if (player == null){ return; }
        int cost = goldShop[itemName];
        player.Purchase(type, itemName, cost);
        updatePlayer[player] = true; // let them know of a new equipment and a new amount of gold next time they ask
        Debug.Log("Purchase complete");
    }
    
    public int[] BattleStats(string token) // returns the player [health, mana, stamina, 0 or 1], last parameter is 1 if they should send a request for player stats which would rebuild max hp, money, etc
    {
        Player player = players[token];
        if (player == null)
        {
            return new int[] { -1, -1, -1, -1 };
        }
        
        int[] battleStats = player.GetStats();
        int[] returnData = new int[4] {battleStats[0], battleStats[1], battleStats[2], updatePlayer[player] ? 1 : 0 };
        
        return returnData;
    }

    // -------------- Player interactions --------------
    public void PlayerKilled(Player murderer, Player murdered)
    {
        // give murderer stolen loot
        Debug.Log("There seems to be a murder");
        murderer.Kill(murdered);
        updatePlayer[murderer] = true; // they will update to more gold next time they ask
        updatePlayer[murdered] = true; // they will lost gold, still need to find a way to reset character
    }

    
    public string WeaponHit(Player user, Player hit, string weaponName, string skillName) // the client tells us when it hits another client
    {
        Weapon weapon = user.CheckWeapon(weaponName);
        if (weapon != null)
        {
            if (!weapon.isAttacking()) // treat this as a new slash and take stamina/mana  
            {
                weapon.Attack(null, user.GetCharge());
            }

            if (weapon.isAttacking()) // this is only false if their attack ran out but the cooldown is still on 
            {
                bool killed = hit.TakeDamage(weapon);
                if (killed)
                {
                    PlayerKilled(user, hit);
                    return "You killed them!";
                }
                return "Damage dealt";
            }
            return "Cooldown is in effect";
        }
        return "Weapon did not exist";
    }

    public string WeaponHit(string token, string hit, string handPos)
    {
        Player user = players[token];
        if (user == null) {return "Invalid token"; }

        Player attacked = null;
        foreach (Player player in players.Values)
        {
            if (player.Username.Equals(hit))
            {
                attacked = player;
            }
        }

        if (attacked == null) { return "Invalid player"; }

        int index = int.Parse(handPos);
        if (index > 1) { return "Invalid weapon"; }
        
        return WeaponHit(user, attacked, user.GetEquipped()[4+index], null);
    }

    // -------------- Private functions --------------
    private void LoadIn(Dictionary<string, Dictionary<string, string>> reference, string[] placements, string text)
    {
        int currentIndex = 0;

        for (int i = 0; i < placements.Length; i++)
        {
            reference[placements[i]] = new Dictionary<string, string> { };
            currentIndex = currentIndex + text.Substring(currentIndex).IndexOf("\n") + 1;

            while (true)
            {
                string name = text.Substring(currentIndex, text.Substring(currentIndex).IndexOf(": "));
                currentIndex += name.Length + 2;

                string info = text.Substring(currentIndex, text.Substring(currentIndex).IndexOf("\n"));
                currentIndex += info.Length + 1;
                
                reference[placements[i]][name] = info;

                if (currentIndex + 3 >= text.Length || text.Substring(currentIndex + 2, 1).Equals("-")) // went through all the lines
                {
                    currentIndex += 3;
                    break;
                }
            }
        }
    }
    
    // -------------- Server Start --------------
    void Start()
    {
        // load in all the asset information into a dictionary
        LoadIn(equipments, new string[] { "Clothing", "Weapons", "Items" }, equipmentFile.text);
        LoadIn(skills, new string[] { "Normal", "Attack", "Magic" }, skillFile.text);
        LoadIn(conditions, new string[] { "All" }, conditionList.text);

        // start the eternal game loop
        (new Thread(GameLoop)).Start();
    }

    // -------------- Server Loop --------------
    private void GameLoop() 
    {
        while (true)
        {
            foreach (KeyValuePair<string, Player> player in players)
            {
                player.Value.UpdateOne();
            }
            Thread.Sleep(100);
        }
    }
}
