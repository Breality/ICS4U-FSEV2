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
    private List<Monster> monsters = new List<Monster> { };
    private List<NPC> npcs = new List<NPC> { };

    public Dictionary<string, Dictionary<string, string>> equipments = new Dictionary<string, Dictionary<string, string>> { };
    public Dictionary<string, Dictionary<string, string>> skills = new Dictionary<string, Dictionary<string, string>> { };
    public Dictionary<string, Dictionary<string, string>> conditions = new Dictionary<string, Dictionary<string, string>> { };

    // -------------- Player decisions sent by HTTP --------------
    public void PlayerEnter(Player player, string token) { players[token] = player; }

    public void PlayerLeave(string token) { players.Remove(token); }

    public void Purchase(string token, string type, string name)
    {

    }


    // -------------- Player interactions --------------
    public void PlayerKilled(Player murderer, Player murdered)
    {

    }

    
    public void WeaponHit(Player user, Player hit, string weaponName, string skillName) // the client tells us when it hits another client
    {
        Weapon weapon = user.CheckWeapon(weaponName);
        if (weapon != null)
        {
            double range = weapon.range;
            double distance = Vector3.Distance(user.GetPos(), hit.GetPos());
            if (weapon.isAttacking() && distance <= range)
            {
                bool killed = hit.TakeDamage(weapon); 
                if (killed)
                {
                    PlayerKilled(user, hit);
                }
            }
        }
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

                Debug.Log(placements[i] + ", " + name + " is " + info);
                equipments[placements[i]][name] = info;

                if (currentIndex + 3 >= text.Length || text.Substring(currentIndex + 2, 1).Equals("-")) // went through all the lines
                {
                    Debug.Log("Done");
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
