// A simple skill effect that appears on the target. We can't just set
// transform.parent because both have a NetworkIdentity, so we need to follow it
//
// Note: Particle Systems need Simulation Space = Local for it to work.
using UnityEngine;
using UnityEngine.Networking;

public class SkillEffectOnTarget : SkillEffect {
    void Update() {
        // follow the target's position.
        if (target != null)
            transform.position = target.collider.bounds.center;

        // destroy self if target disappeared or particle ended
        if (isServer)
            if (target == null || !GetComponent<ParticleSystem>().IsAlive())
                NetworkServer.Destroy(gameObject);
    }
}
