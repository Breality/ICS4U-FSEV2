using System.Collections;
using System.Collections.Generic;
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
        double range = weapon.range;
        double distance = Vector3.Distance(user.getPos(), hit.getPos());
        Attack skill = (skillName == null) ? weapon.skills[skillName] : null;

        if (distance <= range)
        {
            weapon.Attack(hit, skill, user.getCharge()); // the attack verifies cooldown and deals the attack 
        }


    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
