// The Skill struct only contains the dynamic skill properties and a name, so
// that the static properties can be read from the scriptable object. The
// benefits are low bandwidth and easy Player database saving (saves always
// refer to the scriptable skill, so we can change that any time).
//
// Skills have to be structs in order to work with SyncLists.
//
// We implemented the cooldowns in a non-traditional way. Instead of counting
// and increasing the elapsed time since the last cast, we simply set the
// 'end' Time variable to Time.time + cooldown after casting each time. This
// way we don't need an extra Update method that increases the elapsed time for
// each skill all the time.
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public partial struct Skill {
    // name used to reference the database entry (cant save template directly
    // because synclist only support simple types)
    public string name;

    // dynamic stats (cooldowns etc.)
    public bool learned;
    public int level;
    public float castTimeEnd; // server time
    public float cooldownEnd; // server time
    public float buffTimeEnd; // server time

    // constructors
    public Skill(SkillTemplate template) {
        name = template.name;

        // learned only if learned by default
        learned = template.learnDefault;
        level = 1;

        // ready immediately
        castTimeEnd = cooldownEnd = buffTimeEnd = Time.time;
    }

    // does the template still exist?
    public bool TemplateExists() {
        return SkillTemplate.dict.ContainsKey(name);
    }

    // database quest property access etc.
    public SkillTemplate template {
        get { return SkillTemplate.dict[name]; }
    }
    public string category {
        get { return template.category; }
    }
    public int damage {
        get { return template.levels[level-1].damage; }
    }
    public float castTime {
        get { return template.levels[level-1].castTime; }
    }
    public float cooldown {
        get { return template.levels[level-1].cooldown; }
    }
    public float castRange {
        get { return template.levels[level-1].castRange; }
    }
    public float aoeRadius {
        get { return template.levels[level-1].aoeRadius; }
    }
    public int manaCosts {
        get { return template.levels[level-1].manaCosts; }
    }
    public int healsHealth {
        get { return template.levels[level-1].healsHealth; }
    }
    public int healsMana {
        get { return template.levels[level-1].healsMana; }
    }
    public float buffTime {
        get { return template.levels[level-1].buffTime; }
    }
    public int buffsHealthMax {
        get { return template.levels[level-1].buffsHealthMax; }
    }
    public int buffsManaMax {
        get { return template.levels[level-1].buffsManaMax; }
    }
    public int buffsDamage {
        get { return template.levels[level-1].buffsDamage; }
    }
    public int buffsDefense {
        get { return template.levels[level-1].buffsDefense; }
    }
    public float buffsBlockChance {
        get { return template.levels[level-1].buffsBlockChance; }
    }
    public float buffsCriticalChance {
        get { return template.levels[level-1].buffsCriticalChance; }
    }
    public float buffsHealthPercentPerSecond {
        get { return template.levels[level-1].buffsHealthPercentPerSecond; }
    }
    public float buffsManaPercentPerSecond {
        get { return template.levels[level-1].buffsManaPercentPerSecond; }
    }
    public bool followupDefaultAttack {
        get { return template.followupDefaultAttack; }
    }
    public Sprite image {
        get { return template.image; }
    }
    public SkillEffect effectPrefab {
        get { return template.levels[level-1].effectPrefab; }
    }
    public bool learnDefault {
        get { return template.learnDefault; }
    }
    public int requiredLevel {
        get { return template.levels[level-1].requiredLevel; }
    }
    public long requiredSkillExperience {
        get { return template.levels[level-1].requiredSkillExperience; }
    }
    public int maxLevel {
        get { return template.levels.Length; }
    }
    public int upgradeRequiredLevel {
        get { return (level < maxLevel) ? template.levels[level].requiredLevel : 0; }
    }
    public long upgradeRequiredSkillExperience {
        get { return (level < maxLevel) ? template.levels[level].requiredSkillExperience : 0; }
    }

    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // SkillTemplate because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the template.
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME} Lvl {LEVEL}</b>
    Description here...

    Damage: {DAMAGE}
    Cast Time: {CASTTIME}
    Cooldown: {COOLDOWN}
    Cast Range: {CASTRANGE}
    AoE Radius: {AOERADIUS}
    Heals Health: {HEALSHEALTH}
    Heals Mana: {HEALSMANA}
    Buff Time: {BUFFTIME}
    Buffs max Health: {BUFFSHEALTHMAX}
    Buffs max Mana: {BUFFSMANAMAX}
    Buffs damage: {BUFFSDAMAGE}
    Buffs defense: {BUFFSDEFENSE}
    Buffs block: {BUFFSBLOCKCHANCE}
    Buffs critical: {BUFFSCRITICALCHANCE}
    Buffs Health % per Second: {BUFFSHEALTHPERCENTPERSECOND}
    Buffs Mana % per Second: {BUFFSMANAPERCENTPERSECOND}
    Mana Costs: {MANACOSTS}
    */
    public string ToolTip(bool showRequirements = false) {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        var tip = new StringBuilder(template.toolTip);
        tip.Replace("{NAME}", name);
        tip.Replace("{CATEGORY}", category);
        tip.Replace("{LEVEL}", level.ToString());
        tip.Replace("{DAMAGE}", damage.ToString());
        tip.Replace("{CASTTIME}", Utils.PrettySeconds(castTime));
        tip.Replace("{COOLDOWN}", Utils.PrettySeconds(cooldown));
        tip.Replace("{CASTRANGE}", castRange.ToString());
        tip.Replace("{AOERADIUS}", aoeRadius.ToString());
        tip.Replace("{HEALSHEALTH}", healsHealth.ToString());
        tip.Replace("{HEALSMANA}", healsMana.ToString());
        tip.Replace("{BUFFTIME}", Utils.PrettySeconds(buffTime));
        tip.Replace("{BUFFSHEALTHMAX}", buffsHealthMax.ToString());
        tip.Replace("{BUFFSMANAMAX}", buffsManaMax.ToString());
        tip.Replace("{BUFFSDAMAGE}", buffsDamage.ToString());
        tip.Replace("{BUFFSDEFENSE}", buffsDefense.ToString());
        tip.Replace("{BUFFSBLOCKCHANCE}", Mathf.RoundToInt(buffsBlockChance * 100).ToString());
        tip.Replace("{BUFFSCRITICALCHANCE}", Mathf.RoundToInt(buffsCriticalChance * 100).ToString());
        tip.Replace("{BUFFSHEALTHPERCENTPERSECOND}", Mathf.RoundToInt(buffsHealthPercentPerSecond * 100).ToString());
        tip.Replace("{BUFFSMANAPERCENTPERSECOND}", Mathf.RoundToInt(buffsManaPercentPerSecond * 100).ToString());
        tip.Replace("{MANACOSTS}", manaCosts.ToString());

        // addon system hooks
        Utils.InvokeMany(typeof(Skill), this, "ToolTip_", tip);

        // only show requirements if necessary
        if (showRequirements) {
            tip.Append("\n<b><i>Required Level: " + requiredLevel + "</i></b>\n" +
                       "<b><i>Required Skill Exp.: " + requiredSkillExperience + "</i></b>\n");
        }
        // only show upgrade if necessary (not if not learned yet etc.)
        if (learned && level < maxLevel) {
            tip.Append("\n<i>Upgrade:</i>\n" +
                       "<i>  Required Level: " + upgradeRequiredLevel + "</i>\n" +
                       "<i>  Required Skill Exp.: " + upgradeRequiredSkillExperience + "</i>\n");
        }

        return tip.ToString();
    }

    public float CastTimeRemaining() {
        // how much time remaining until the casttime ends? (using server time)
        return NetworkTime.time >= castTimeEnd ? 0 : castTimeEnd - NetworkTime.time;
    }

    public bool IsCasting() {
        // we are casting a skill if the casttime remaining is > 0
        return CastTimeRemaining() > 0;
    }

    public float CooldownRemaining() {
        // how much time remaining until the cooldown ends? (using server time)
        return NetworkTime.time >= cooldownEnd ? 0 : cooldownEnd - NetworkTime.time;
    }

    public float BuffTimeRemaining() {
        // how much time remaining until the buff ends? (using server time)
        return NetworkTime.time >= buffTimeEnd ? 0 : buffTimeEnd - NetworkTime.time;
    }

    public bool IsReady() {
        return CooldownRemaining() == 0;
    }
}

public class SyncListSkill : SyncListStruct<Skill> { }
