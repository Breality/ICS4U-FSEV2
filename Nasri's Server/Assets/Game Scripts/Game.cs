// Noor's Server
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// handles game logic. Animations pass right through from client to client, position checks occur here and static game logic takes place. 
public class Game : MonoBehaviour
{
    private Dictionary<string, Player> players = new Dictionary<string, Player> { }; // {token, player}
    private List<Monster> monsters = new List<Monster> { };
    private List<NPC> npcs = new List<NPC> { };

    public void WeaponHit(Player user, Player hit, string weaponName, string skillName)
    {
        Weapon weapon = user.checkWeapon(weaponName);
        if (weapon != null)
        {
            double range = weapon.range;
            double distance = Vector3.Distance(user.getPos(), hit.getPos());
            Attack skill = (skillName == null) ? weapon.skills[skillName] : null;

            if (distance <= range && user.getStats()[1] >= weapon.mana && user.getStats()[2] >= weapon.stamina)
            {
                weapon.Attack(hit, skill, user.getCharge()); // the attack verifies cooldown and deals the attack 
            }
        }
        
    }

    private void GameLoop() // every frame of the game for npc redirection, heal effects, etc. Instant things like movement are done via events 
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

    // Start is called before the first frame update
    void Start()
    {
        (new Thread(GameLoop)).Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
