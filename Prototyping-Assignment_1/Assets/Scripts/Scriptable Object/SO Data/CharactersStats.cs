using UnityEngine;
using System.Collections;
using JetBrains.Annotations;


// Base data for all characters

[CreateAssetMenu(fileName = "New Character", menuName = "Character/CharacterStats")]
public class CharactersStats : ScriptableObject
{
    [Tooltip("Character Name")]
    public string charName;

    [Tooltip("Health")]
    public int Vigor;

    [Tooltip("Damage per hit")]
    public int Strength;

    [Tooltip("Defense points")]
    public int Endurance;

    [Tooltip("Speed points")]
    public float Agility;
}   
    