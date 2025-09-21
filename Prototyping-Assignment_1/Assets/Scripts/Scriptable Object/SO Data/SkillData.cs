using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Character/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Ability Name")]
    public string skillName;

    [Header("Usable Position")]
    public bool canUsefront;
    public bool canUseMiddle;
    public bool canUseBack;
    public bool canUseAny;

    [Header("Can Target")]
    public bool canTargetfront;
    public bool canTargetMiddle;
    public bool canTargetBack;
    public bool canTargetAny;

    [Header("Damage")]
    public int skillDamage;

    [Header("Critical Chance")]
    public float critChance;

    [Header("Movement")]
    public int moveFoward;
    public int moveBackward;

    [Header("Can Miss")]
    public bool canMiss;

    [Header("Special Rule")]
    public bool canHeal;
    public bool healSelf;
    public bool parry;
    public bool halfDamageSelf;
    public bool halfDamage;
    
    
}
