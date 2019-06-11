// Base component for skill effects.
//
// There are no limitations whatsoever. Create a prefab, attach your script of
// type SkillEffect and then attach any additional particles/scripts necessary.
//
// Examples:
// - Arrows: take a look at the Arrow prefab
// - Buffs: a NetworkIdentity can't have a NetworkIdentity child, so a buff
//   SkillEffect would have to auto follow the Player while the buff is active.
// - Jumping Thunderbolt: like the Arrow script, but following the next nearest
//   attackable entity afterwards.
//
// About Server/Client simulation:
//   There is a useful optimization that we can do to save lots of bandwidth:
//   By default, we always do all the logic on the server and then just synchro-
//   nize the position to the client via NetworkTransform. This is perfectly
//   fine and you should do that to be save.
//
//   It's important to know that most effects can be done without any synchroni-
//   zations, saving lots of bandwidth. For example:
//   - An arrow just flies to the target with some speed. We can do that on the
//     client and it will be the same result as on the server.
//   - Even a lightning strike that jumps to other entities can be done without
//     any NetworkTransform if we assume that it always jumps to the closest
//     entity. That will be the same on the server and on the client.
//   - A lightning strike that jumps to a RANDOM entity has to be done on the
//     server and synchronized via NetworkTransform. Calling Random from the
//     client would cause it to jump to different entities on each client.
//
//   In other words: use 'if (isServer)' to simulate all the logic and use
//   NetworkTransform to synchronize it to clients. Buf if you are an expert,
//   you might as well avoid NetworkTransform and simulate on server and client.
//
// Notes:
// - Make sure to drag all your SkillEffect prefabs into the NetworkManager
//   spawnable prefabs list.
// - Effects can either be simulated on the server and then synchronized via
//   NetworkTransform, or simulated on the server and on the client without
//   NetworkTransform. The latter option makes most sense to save bandwidth, but
//   for random effects it would have to be simulated on the server, since the
//   client and the server would both get different results when calling Random.
using UnityEngine;
using UnityEngine.Networking;

public abstract class SkillEffect : NetworkBehaviour {
    // some effects are just visual, like blood splatter. while other effects
    // want to take care of the logic (like dealing damage) by themselves.
    // the Entity script needs to know that, so it doesn't deal damage twice.
    public bool isPurelyVisual = true;

    // [SyncVar] NetworkIdentity: errors when null
    // [SyncVar] Entity: SyncVar only works for simple types
    // [SyncVar] GameObject is the only solution where we don't need a custom
    //           synchronization script (needs NetworkIdentity component!)
    // -> we still wrap it with a property for easier access, so we don't have
    //    to use target.GetComponent<Entity>() everywhere
    [SyncVar, HideInInspector] GameObject _target;
    public Entity target {
        get { return _target != null  ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }

    [SyncVar, HideInInspector] GameObject _caster;
    public Entity caster {
        get { return _caster != null  ? _caster.GetComponent<Entity>() : null; }
        set { _caster = value != null ? value.gameObject : null; }
    }

    // skill that we casted
    [SyncVar, HideInInspector] public int skillIndex;
    public Skill sourceSkill {
        get { return caster.skills[skillIndex]; }
    }
}
