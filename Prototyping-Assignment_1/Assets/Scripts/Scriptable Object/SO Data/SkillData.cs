using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Character/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int damage;
    public bool isHeal;
    public bool isBuff;
    public float hitChance = 0.8f;

    public TurnOrder.Positions[] targetPositions;

    public int MoveSelf;
    public int MoveTarget;
}