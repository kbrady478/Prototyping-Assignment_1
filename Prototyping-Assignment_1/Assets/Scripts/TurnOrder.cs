using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

// this code runs the turn orders
// runs whos turn it is using a state machine 

public class TurnOrder : MonoBehaviour
{
    // FUNCTIONS/ METHODS SHOULD.....

    // TRACK POSITION OF EACH CHARACTER
    // PLAYER CHOICE (Attack, Skill, Parry.....)

    // TRACK TURN ORDER
    // DETERMINED ENEMY AI CHOICE
    // INTERACTION OF SAID CHOICES
    // ACTION PHASE FOR SAID CHOICES


// ====== Scriptable Objects ======
    // Grab from the instance variable from the SO scripts
    public CharactersStats Mage;
    public CharactersStats Gunslinger;
    public CharactersStats Prince;
    public CharactersStats Goblin;

#region Unit/Character
// ====== UNIT / CHARACTER =======
    public enum Positions
    {
        Front,
        Middle,
        Back
    }
   
    public class Unit
    {
        // Here Should track said units data (character stats, Positions, state Player || Enemy )
        // Remember Dead positions, IF Miss.... return otherwise do said actions

        // Unit Data
        public CharactersStats Stats;
        public int CurrentHealth;
        public Positions CurrentPositions;
        public bool IsPlayer;
        public Transform initialstart;


        public Unit(CharactersStats stats, Positions startPos, bool isPlayer, Transform initialStart)
        {
            Stats = stats;
            CurrentHealth = stats.Vigor;
            CurrentPositions = startPos;
            IsPlayer = isPlayer;
            initialstart = initialStart;
        }
    }
#endregion

// ====== Player Actions =======
    // Should control the action the player does using buttons in the ui
    // Resolves said actions in a waiting queue list



// ====== State Machine =======
    // Using switch cases to resolve the phases of order

    private enum TurnState
    {
        PlayerChoice,
        EnemyChoice,
        Resolve
    }
    private TurnState currentState;

// ====== Turn Order ======
    // Handles the full turn order
    // Using lists to keep the currentState, positions, and action queues
    // end with Action Phase
}