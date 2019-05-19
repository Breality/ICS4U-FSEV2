using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    // Identifiers < Store on login
    public readonly string Username;
    private readonly string Token;

    // Player's inventory < Save and Load. Clothing, Weapon, and Item all inherit from Equipment.
    private string[] equipped = new string[6]; // 0 = Helmet, 1 = Body Armour, 2 = Boots, 3 = Pendulum, 4 = Weapon 1, 5 = Weapon 2 
    private Dictionary<string, Clothing> clothing = new Dictionary<string, Clothing> { };
    private Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon> { };
    private Dictionary<string, Item> items = new Dictionary<string, Item> { };
    
    // Game stats < Save and Load
    private int gold = 0;
    private List<string> titles = new List<string> { };
    private string chosenTitle = "Player";
    private int score = 0; // for world leaderboard

    // Quest stats < Save and Load
    private int mainProgress = 0; // the progress in the main storyline
    private Dictionary<string, int> optionalQuests = new Dictionary<string, int> { }; // the quests they have taken and their progress on them. -1 for completed

    // Special Skills < Save and Load. Magic, Attack and Ability all inherit from Skill
    private Dictionary<string, Magic> magicSpells = new Dictionary<string, Magic> { };
    private Dictionary<string, Attack> attackSkills = new Dictionary<string, Attack> { };
    private Dictionary<string, Ability> playerAbilities = new Dictionary<string, Ability> { };

    // Combat stats < Recalculate when needed
    private int[] hp = new int[] { 100, 100 }; // {max, current}
    private int[] mana = new int[] { 100, 100 }; // {max, current}
    private int[] stamina = new int[] { 100, 100 }; // {max, current}

    private float resist = 0;
    private float[] attackPower = new float[] { 0, 1 }; // {bonus, multiplier}
    private float[] magicPower = new float[] { 0, 1 }; // {bonus, multiplier}

    private int mobility = 20;

    // Game variables for playing
    public static readonly Dictionary<string, double> staminaCosts = new Dictionary<string, double> { { "Idle", 0 }, {"Walking", 1}, {"Sprinting", 3} };
    private string status = "idle";

    public Player(string playerInfo)
    {

    }

    public void UpdateOne() // deals with health regen, poison effects, whatever
    {
        hp[1] = Mathf.Min(hp[0], hp[1] + hp[0] / 100);
    }
}