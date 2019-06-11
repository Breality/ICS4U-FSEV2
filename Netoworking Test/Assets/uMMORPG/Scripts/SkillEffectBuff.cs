// A simple skill effect that follows the target until it ends.
// -> Can be used for buffs.
//
// Note: Particle Systems need Simulation Space = Local for it to work.
using UnityEngine;
using UnityEngine.Networking;

public class SkillEffectBuff : SkillEffect {
    float lastRemainingTime = Mathf.Infinity;

    void Update() {
        // only while target still exists, buff still active and hasn't been
        // recasted
        if (target != null &&
            sourceSkill.BuffTimeRemaining() > 0 &&
            lastRemainingTime >= sourceSkill.BuffTimeRemaining()) {
            transform.position = target.collider.bounds.center;
            lastRemainingTime = sourceSkill.BuffTimeRemaining();
        } else {
            if (isServer) NetworkServer.Destroy(gameObject);
        }
    }
}
