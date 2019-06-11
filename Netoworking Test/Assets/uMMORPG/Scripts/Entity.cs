// The Entity class is rather simple. It contains a few basic entity properties
// like health, mana and level that all inheriting classes like Players and
// Monsters can use.
//
// Entities also have a _target_ Entity that can't be synchronized with a
// SyncVar. Instead we created a EntityTargetSync component that takes care of
// that for us.
//
// Entities use a deterministic finite state machine to handle IDLE/MOVING/DEAD/
// CASTING etc. states and events. Using a deterministic FSM means that we react
// to every single event that can happen in every state (as opposed to just
// taking care of the ones that we care about right now). This means a bit more
// code, but it also means that we avoid all kinds of weird situations like 'the
// monster doesn't react to a dead target when casting' etc.
// The next state is always set with the return value of the UpdateServer
// function. It can never be set outside of it, to make sure that all events are
// truly handled in the state machine and not outside of it. Otherwise we may be
// tempted to set a state in CmdBeingTrading etc., but would likely forget of
// special things to do depending on the current state.
//
// Entities also need a kinematic Rigidbody so that OnTrigger functions can be
// called. Note that there is currently a Unity bug that slows down the agent
// when having lots of FPS(300+) if the Rigidbody's Interpolate option is
// enabled. So for now it's important to disable Interpolation - which is a good
// idea in general to increase performance.
using UnityEngine;
#if UNITY_5_5_OR_NEWER // for people that didn't upgrade to 5.5. yet
using UnityEngine.AI;
#endif
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

// note: no animator required, towers, dummies etc. may not have one
[RequireComponent(typeof(Rigidbody))] // kinematic, only needed for OnTrigger
[RequireComponent(typeof(NetworkProximityCheckerCustom))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
public abstract partial class Entity : NetworkBehaviour {
    [Header("Components")]
    public NavMeshAgent agent;
    public NetworkProximityChecker proxchecker;
    public NetworkIdentity netIdentity;
    public Animator animator;
    new public Collider collider;

    // finite state machine
    // -> state only writable by entity class to avoid all kinds of confusion
    [Header("State")]
    [SyncVar, SerializeField] string _state = "IDLE";
    public string state { get { return _state; } }

    // [SyncVar] NetworkIdentity: errors when null
    // [SyncVar] Entity: SyncVar only works for simple types
    // [SyncVar] GameObject is the only solution where we don't need a custom
    //           synchronization script (needs NetworkIdentity component!)
    // -> we still wrap it with a property for easier access, so we don't have
    //    to use target.GetComponent<Entity>() everywhere
    [Header("Target")]
    [SyncVar] GameObject _target;
    public Entity target {
        get { return _target != null  ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }

    [Header("Level")]
    [SyncVar] public int level = 1;

    [Header("Health")]
    public bool invincible = false; // GMs, Npcs, ...
    [SyncVar] int _health = 1;
    public int health {
        get { return Mathf.Min(_health, healthMax); } // min in case hp>hpmax after buff ends etc.
        set { _health = Mathf.Clamp(value, 0, healthMax); }
    }
    public abstract int healthMax{ get; }
    public bool healthRecovery = true; // can be disabled in combat etc.
    public int baseHealthRecoveryRate = 1;
    public int healthRecoveryRate {
        get {
            // base + buffs
            float buffPercent = (from skill in skills
                                 where skill.BuffTimeRemaining() > 0
                                 select skill.buffsHealthPercentPerSecond).Sum();
            return baseHealthRecoveryRate + Convert.ToInt32(buffPercent * healthMax);
        }
    }

    [Header("Mana")]
    [SyncVar] int _mana = 1;
    public int mana {
        get { return Mathf.Min(_mana, manaMax); } // min in case hp>hpmax after buff ends etc.
        set { _mana = Mathf.Clamp(value, 0, manaMax); }
    }
    public abstract int manaMax{ get; }
    public bool manaRecovery = true; // can be disabled in combat etc.
    public int baseManaRecoveryRate = 1;
    public int manaRecoveryRate {
        get {
            // base + buffs
            float buffPercent = (from skill in skills
                                 where skill.BuffTimeRemaining() > 0
                                 select skill.buffsManaPercentPerSecond).Sum();
            return baseManaRecoveryRate + Convert.ToInt32(buffPercent * manaMax);
        }
    }

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;

    // other properties
    public float speed { get { return agent.speed; } }
    public abstract int damage { get; }
    public abstract int defense { get; }
    public abstract float blockChance { get; }
    public abstract float criticalChance { get; }

    // skill system for all entities (players, monsters, npcs, towers, ...)
    // 'skillTemplates' are the available skills (first one is default attack)
    // 'skills' are the loaded skills with cooldowns etc.
    [Header("Skills, Buffs, Status Effects")]
    public SkillTemplate[] skillTemplates;
    public SyncListSkill skills = new SyncListSkill();
    // current skill (synced because we need it as an animation parameter)
    [SyncVar] protected int currentSkill = -1;

    // all entities should have an inventory, not just the player.
    // useful for monster loot, chests, etc.
    [Header("Inventory")]
    public SyncListItem inventory = new SyncListItem();

    // all entities should have gold, not just the player
    // useful for monster loot, chests etc.
    // note: int is not enough (can have > 2 mil. easily)
    [Header("Gold")]
    [SyncVar, SerializeField] long _gold = 0;
    public long gold { get { return _gold; } set { _gold = Math.Max(value, 0); } }

    // networkbehaviour ////////////////////////////////////////////////////////
    protected virtual void Awake() {
        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "Awake_");
    }

    public override void OnStartServer() {
        // health recovery every second
        InvokeRepeating("Recover", 1, 1);

        // dead if spawned without health
        if (health == 0) _state = "DEAD";

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "OnStartServer_");
    }

    protected virtual void Start() {
        // disable animator on server. this is a huge performance boost and
        // definitely worth one line of code (1000 monsters: 22 fps => 32 fps)
        // (!isClient because we don't want to do it in host mode either)
        // (OnStartServer doesn't know isClient yet, Start is the only option)
        if (!isClient) animator.enabled = false;
    }

    // monsters, npcs etc. don't have to be updated if no player is around
    // checking observers is enough, because lonely players have at least
    // themselves as observers, so players will always be updated
    // and dead monsters will respawn immediately in the first update call
    // even if we didn't update them in a long time (because of the 'end'
    // times)
    // -> update only if:
    //    - observers are null (they are null in clients)
    //    - if they are not null, then only if at least one (on server)
    //    - if the entity is hidden, otherwise it would never be updated again
    //      because it would never get new observers
    public bool IsWorthUpdating() {
        return netIdentity.observers == null ||
               netIdentity.observers.Count > 0 ||
               IsHidden();
    }

    // entity logic will be implemented with a finite state machine
    // -> we should react to every state and to every event for correctness
    // -> we keep it functional for simplicity
    // note: can still use LateUpdate for Updates that should happen in any case
    void Update() {
        // only update if it's worth updating (see IsWorthUpdating comments)
        // -> we also clear the target if it's hidden, so that players don't
        //    keep hidden (respawning) monsters as target, hence don't show them
        //    as target again when they are shown again
        if (IsWorthUpdating()) {
            if (isClient) UpdateClient();
            if (isServer) {
                if (target != null && target.IsHidden()) target = null;
                _state = UpdateServer();
            }

            // addon system hooks
            Utils.InvokeMany(typeof(Entity), this, "Update_");
        }
    }

    // update for server. should return the new state.
    protected abstract string UpdateServer();

    // update for client.
    protected abstract void UpdateClient();

    // visibility //////////////////////////////////////////////////////////////
    // hide a entity
    // note: using SetActive won't work because its not synced and it would
    //       cause inactive objects to not receive any info anymore
    // note: this won't be visible on the server as it always sees everything.
    [Server]
    public void Hide() {
        proxchecker.forceHidden = true;
    }

    [Server]
    public void Show() {
        proxchecker.forceHidden = false;
    }

    // is the entity currently hidden?
    // note: usually the server is the only one who uses forceHidden, the
    //       client usually doesn't know about it and simply doesn't see the
    //       GameObject.
    public bool IsHidden() {
        return proxchecker.forceHidden;
    }

    public float VisRange() {
        return proxchecker.visRange;
    }

    // look at a transform while only rotating on the Y axis (to avoid weird
    // tilts)
    public void LookAtY(Vector3 position) {
        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
    }

    // note: client can find out if moving by simply checking the state!
    [Server] // server is the only one who has up-to-date NavMeshAgent
    public bool IsMoving() {
        // -> agent.hasPath will be true if stopping distance > 0, so we can't
        //    really rely on that.
        // -> pathPending is true while calculating the path, which is good
        // -> remainingDistance is the distance to the last path point, so it
        //    also works when clicking somewhere onto a obstacle that isn'
        //    directly reachable.
        return agent.pathPending ||
               agent.remainingDistance > agent.stoppingDistance ||
               agent.velocity != Vector3.zero;
    }

    // health & mana ///////////////////////////////////////////////////////////
    public float HealthPercent() {
        return (health != 0 && healthMax != 0) ? (float)health / (float)healthMax : 0;
    }

    [Server]
    public void Revive(float healthPercentage = 1) {
        health = Mathf.RoundToInt(healthMax * healthPercentage);
    }

    public float ManaPercent() {
        return (mana != 0 && manaMax != 0) ? (float)mana / (float)manaMax : 0;
    }

    // combat //////////////////////////////////////////////////////////////////
    // no need to instantiate damage popups on the server
    enum PopupType { Normal, Block, Crit };
    [ClientRpc(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void RpcShowDamagePopup(PopupType popupType, int amount, Vector3 position) {
        // spawn the damage popup (if any) and set the text
        // (-1 = block)
        if (damagePopupPrefab) {
            var popup = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            if (popupType == PopupType.Normal)
                popup.GetComponentInChildren<TextMesh>().text = amount.ToString();
            else if (popupType == PopupType.Block)
                popup.GetComponentInChildren<TextMesh>().text = "<i>Block!</i>";
            else if (popupType == PopupType.Crit)
                popup.GetComponentInChildren<TextMesh>().text = amount + " Crit!";
        }
    }

    // deal damage at another entity
    // (can be overwritten for players etc. that need custom functionality)
    // (can also return the set of entities that were hit, just in case they are
    //  needed when overwriting it)
    [Server]
    public virtual HashSet<Entity> DealDamageAt(Entity entity, int amount, float aoeRadius=0) {
        // build the set of entities that were hit within AoE range
        var entities = new HashSet<Entity>();

        // add main target in any case, because non-AoE skills have radius=0
        entities.Add(entity);

        // add all targets in AoE radius around main target
        var colliders = Physics.OverlapSphere(entity.transform.position, aoeRadius); //, layerMask);
        foreach (var co in colliders) {
            var candidate = co.GetComponentInParent<Entity>();
            // overlapsphere cast uses the collider's bounding volume (see
            // Unity scripting reference), hence is often not exact enough
            // in our case (especially for radius 0.0). let's also check the
            // distance to be sure.
            if (candidate != null && candidate != this && candidate.health > 0 &&
                Vector3.Distance(entity.transform.position, candidate.transform.position) < aoeRadius)
                entities.Add(candidate);
        }

        // now deal damage at each of them
        foreach (var e in entities) {
            int damageDealt = 0;
            var popupType = PopupType.Normal;

            // don't deal any damage if target is invincible
            if (!e.invincible) {
                // block? (we use < not <= so that block rate 0 never blocks)
                if (UnityEngine.Random.value < e.blockChance) {
                    popupType = PopupType.Block;
                // deal damage
                } else {
                    // subtract defense (but leave at least 1 damage, otherwise
                    // it may be frustrating for weaker players)
                    damageDealt = Mathf.Max(amount - e.defense, 1);

                    // critical hit?
                    if (UnityEngine.Random.value < criticalChance) {
                        damageDealt *= 2;
                        popupType = PopupType.Crit;
                    }

                    // deal the damage
                    e.health -= damageDealt;
                }
            }

            // show damage popup in observers via ClientRpc
            // showing them above their head looks best, and we don't have to
            // use a custom shader to draw world space UI in front of the entity
            // note: we send the RPC to ourselves because whatever we killed
            //       might disappear before the rpc reaches it
            var bounds = e.collider.bounds;
            RpcShowDamagePopup(popupType, damageDealt, new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));

            // let's make sure to pull aggro in any case so that archers
            // are still attacked if they are outside of the aggro range
            e.OnAggro(this);
        }

        // addon system hooks
        Utils.InvokeMany(typeof(Entity), this, "DealDamageAt_", entities, amount);

        return entities;
    }

    // recovery ////////////////////////////////////////////////////////////////
    // recover health and mana once a second
    // note: when stopping the server with the networkmanager gui, it will
    //       generate warnings that Recover was called on client because some
    //       entites will only be disabled but not destroyed. let's not worry
    //       about that for now.
    [Server]
    public void Recover() {
        if (enabled && health > 0) {
            if (healthRecovery) health += healthRecoveryRate;
            if (manaRecovery) mana += manaRecoveryRate;
        }
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by the AggroArea (if any) on clients and server
    public virtual void OnAggro(Entity entity) {}

    // skill system ////////////////////////////////////////////////////////////
    // helper function to find a skill index
    public int GetSkillIndexByName(string skillName) {
        return skills.FindIndex(skill => skill.learned && skill.name == skillName);
    }

    // fist fights are virtually pointless because they overcomplicate the code
    // and they don't add any value to the game. so we need a check to find out
    // if the entity currently has a weapon equipped, otherwise casting a skill
    // shouldn't be possible. this may always return true for monsters, towers
    // etc.
    public abstract bool HasCastWeapon();

    // we can't have a public array of types that we can modify in the Inspector
    // so we need an abstract function to check if players can attack players,
    // monsters, npcs etc.
    public abstract bool CanAttackType(Type type);

    // the first check validates the caster
    // (the skill won't be ready if we check self while casting it. so the
    //  checkSkillReady variable can be used to ignore that if needed)
    public bool CastCheckSelf(Skill skill, bool checkSkillReady = true) {
        // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
        return HasCastWeapon() &&
               (!checkSkillReady || skill.IsReady()) &&
               health > 0 &&
               mana >= skill.manaCosts;
    }

    // the second check validates the target and corrects it for the skill if
    // necessary (e.g. when trying to heal an npc, it sets target to self first)
    public bool CastCheckTarget(Skill skill) {
        // attack: target exists, alive, not self, oktype
        // (we can't have a public array of types that we can modify
        //  in the Inspector, so we need an abstract function)
        if (skill.category == "Attack") {
            return target != null &&
                   target != this &&
                   target.health > 0 &&
                   CanAttackType(target.GetType());
        // heal: on target? (if exists, not self, type) or self
        } else if (skill.category == "Heal") {
            if (target != null &&
                target != this &&
                target.GetType() == GetType()) {
                // can only heal the target if it's not dead
                return target.health > 0;
            // otherwise we want to heal ourselves, which is always allowed
            // (we already checked if we are alive in castcheckself)
            } else {
                target = this;
                return true;
            }
        // buff: only buff self => ok
        } else if (skill.category == "Buff") {
            target = this;
            return true;
        }
        // otherwise the category is invalid
        Debug.LogWarning("invalid skill category for: " + skill.name);
        return false;
    }

    // the third check validates the distance between the caster and the target
    // (in case of buffs etc., the target was already corrected to 'self' by
    //  castchecktarget, hence we don't have to worry about anything here)
    public bool CastCheckDistance(Skill skill) {
        return target != null &&
               Utils.ClosestDistance(collider, target.collider) <= skill.castRange;
    }

    // casts the skill. casting and waiting has to be done in the state machine
    public void CastSkill(Skill skill) {
        // check self again (alive, mana, weapon etc.). ignoring the skill cd
        // and check target again
        // note: we don't check the distance again. the skill will be cast even
        // if the target walked a bit while we casted it (it's simply better
        // gameplay and less frustrating)
        if (CastCheckSelf(skill, false) && CastCheckTarget(skill)) {
            // do the logic in here or let the skill effect take care of it?
            if (skill.effectPrefab == null || skill.effectPrefab.isPurelyVisual) {
                // attack
                if (skill.category == "Attack") {
                    // deal damage directly
                    DealDamageAt(target, damage + skill.damage, skill.aoeRadius);
                // heal
                } else if (skill.category == "Heal") {
                    // note: 'target alive' checks were done above already
                    target.health += skill.healsHealth;
                    target.mana += skill.healsMana;
                // buff
                } else if (skill.category == "Buff") {
                    // set the buff end time (the rest is done in .damage etc.)
                    skill.buffTimeEnd = Time.time + skill.buffTime;
                }
            }

            // spawn the skill effect (if any)
            SpawnSkillEffect(currentSkill, target);

            // decrease mana in any case
            mana -= skill.manaCosts;

            // start the cooldown (and save it in the struct)
            skill.cooldownEnd = Time.time + skill.cooldown;

            // save any skill modifications in any case
            skills[currentSkill] = skill;
        } else {
            // not all requirements met. no need to cast the same skill again
            currentSkill = -1;
        }
    }

    public void SpawnSkillEffect(int skillIndex, Entity effectTarget) {
        // spawn the skill effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        var skill = skills[skillIndex];
        if (skill.effectPrefab != null) {
            var mount = transform.FindRecursively("EffectMount");
            var position = mount != null ? mount.position : transform.position;
            var go = Instantiate(skill.effectPrefab.gameObject, position, Quaternion.identity);
            var effect = go.GetComponent<SkillEffect>();
            effect.target = effectTarget;
            effect.caster = this;
            effect.skillIndex = skillIndex;
            NetworkServer.Spawn(go);
        }
    }

    // helper function to stop all buffs if needed (e.g. in OnDeath)
    public void StopBuffs() {
        for (int i = 0; i < skills.Count; ++i) {
            if (skills[i].category == "Buff") { // not for Murder status etc.
                var skill = skills[i];
                skill.buffTimeEnd = Time.time;
                skills[i] = skill;
            }
        }
    }

    // inventory ///////////////////////////////////////////////////////////////
    // helper function to find an item in the inventory
    public int GetInventoryIndexByName(string itemName) {
        return inventory.FindIndex(item => item.valid && item.name == itemName);
    }

    // helper function to calculate the free slots
    public int InventorySlotsFree() {
        return inventory.Count (item => !item.valid);
    }

    // helper function to calculate the total amount of an item type in inentory
    public int InventoryCountAmount(string itemName) {
        return (from item in inventory
                where item.valid && item.name == itemName
                select item.amount).Sum();
    }

    // helper function to remove 'n' items from the inventory
    public bool InventoryRemoveAmount(string itemName, int amount) {
        for (int i = 0; i < inventory.Count; ++i) {
            if (inventory[i].valid && inventory[i].name == itemName) {
                var item = inventory[i];

                // take as many as possible
                int take = Mathf.Min(amount, item.amount);
                item.amount -= take;
                amount -= take;

                // make slot invalid if amount is 0 now
                if (item.amount == 0) item.valid = false;

                // save all changes
                inventory[i] = item;

                // are we done?
                if (amount == 0) return true;
            }
        }

        // if we got here, then we didn't remove enough items
        return false;
    }

    // helper function to check if the inventory has space for 'n' items of type
    // -> the easiest solution would be to check for enough free item slots
    // -> it's better to try to add it onto existing stacks of the same type
    //    first though
    // -> it could easily take more than one slot too
    // note: this checks for one item type once. we can't use this function to
    //       check if we can add 10 potions and then 10 potions again (e.g. when
    //       doing player to player trading), because it will be the same result
    public bool InventoryCanAddAmount(ItemTemplate item, int amount) {
        // go through each slot
        for (int i = 0; i < inventory.Count; ++i) {
            // empty? then subtract maxstack
            if (!inventory[i].valid)
                amount -= item.maxStack;
            // not empty and same type? then subtract free amount (max-amount)
            else if (inventory[i].valid && inventory[i].name == item.name)
                amount -= (inventory[i].maxStack - inventory[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }

    // helper function to put 'n' items of a type into the inventory, while
    // trying to put them onto existing item stacks first
    // -> this is better than always adding items to the first free slot
    // -> function will only add them if there is enough space for all of them
    public bool InventoryAddAmount(ItemTemplate item, int amount) {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (InventoryCanAddAmount(item, amount)) {
            // go through each slot
            for (int i = 0; i < inventory.Count; ++i) {
                // empty? then fill slot with as many as possible
                if (!inventory[i].valid) {
                    int add = Mathf.Min(amount, item.maxStack);
                    inventory[i] = new Item(item, add);
                    amount -= add;
                }
                // not empty and same type? then add free amount (max-amount)
                else if (inventory[i].valid && inventory[i].name == item.name) {
                    int space = inventory[i].maxStack - inventory[i].amount;
                    int add = Mathf.Min(amount, space);
                    var temp = inventory[i];
                    temp.amount += add;
                    inventory[i] = temp;
                    amount -= add;
                }

                // were we able to fit the whole amount already?
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return false;
    }
}
