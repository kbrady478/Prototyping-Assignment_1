using UnityEngine;
using System.Collections;


// Base data for all characters

[CreateAssetMenu(fileName = "New Character", menuName = "Character/CharacterStats")]
public class CharactersStats : ScriptableObject
{
    [Tooltip("Character Name")]
    public string charName;

    [Tooltip("Health")]
    public int Vigor;

    [Tooltip("Defense")]
    public int Endurance;

    [Tooltip("Multiplier per hit value")]
    public float Critical;

    [Tooltip("Speed")]
    public float Agility;
}   
    