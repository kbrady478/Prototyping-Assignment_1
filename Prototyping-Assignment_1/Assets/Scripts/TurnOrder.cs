using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;

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

        public Unit(CharactersStats stats, Positions startPos, bool isPlayer)
        {
            Stats = stats;
            CurrentHealth = stats.Vigor;
            CurrentPositions = startPos;
            IsPlayer = isPlayer;
        }

        public bool IsDead() => CurrentHealth <= 0;

        public void TakeDamage(int damage)
        {
            if (IsDead()) return;
            {
                // Damage check if lower than Endurance
                int actualDamage = Mathf.Max(damage - Stats.Endurance, 0);
                CurrentHealth -= actualDamage;


                Debug.Log(Stats.charName + " took -" + actualDamage + " damage. Vigor =" + CurrentHealth);

            }
        }

    }
    #endregion

    #region Player Actions
    // ====== Player Actions =======
    // Should control the action the player does using buttons in the ui
    // Resolves said actions in a waiting queue list
    public enum PlayerAction
    {
        // Add more here for other skills
        Attack
    }

    public class PlayerActionType
    {
        public PlayerAction Skill;
        public Positions TargetPos;

        public PlayerActionType(PlayerAction skill, Positions target)
        {
            Skill = skill;
            TargetPos = target;
        }
    }

    #endregion



    #region State Machine
    // ====== State Machine =======
    // Using switch cases to resolve the phases of order
    private enum TurnState
    {
        PlayerChoice,
        EnemyChoice,
        Resolve
    }
    private TurnState currentState;
    #endregion

    #region Turn Order
    // ====== Turn Order =======
    // Handles the full turn order code
    // Using lists to keep the currentState, positions, and action queues
    // end with Action Phase

    private List<Unit> players = new List<Unit>();
    private List<Unit> enemies = new List<Unit>();

    private Queue<PlayerActionType> playerActionsQueue = new Queue<PlayerActionType>();
    private Queue<PlayerActionType> enemyActionsQueue = new Queue<PlayerActionType>();

    private int currentTurnOrder = 0;

    // ======= Starting Order ========== 
    // BACK MIDDLE FRONT FRONT MIDDLE BACK
    // MAGE GUNSL  PRIN  GOB   GOB    GOB
    // ENEMY > PLAYER > ACTION
    private void Start()
    {
        // Player Side
        players.Add(new Unit(Mage, Positions.Back, true));
        players.Add(new Unit(Gunslinger, Positions.Middle, true));
        players.Add(new Unit(Prince, Positions.Front, true));

        // Enemy Side
        enemies.Add(new Unit(Goblin, Positions.Front, false));
        enemies.Add(new Unit(Goblin, Positions.Middle, false));
        enemies.Add(new Unit(Goblin, Positions.Back, false));

        // Shows the current what happens
        PreviewTurn();
    }

    // ====== Preview Turn Order =====
    private void PreviewTurn()
    {
        Debug.Log("===== Preview turn ======");
        currentTurnOrder = 0;
        EnemyTurn();
        currentState = TurnState.PlayerChoice;
    }


    //  ======== Player Choice =========
    public void PlayerTurn(int targetPosition)
    {
        Debug.Log("===== Player turn ======");
        Positions target = (Positions)targetPosition;

        Unit currentPlayer = players[currentTurnOrder];

        // Simple hit chance (80%)
        bool isHit = UnityEngine.Random.value <= 0.8f;

        if (isHit)
        {
            PlayerActionType action = new PlayerActionType(PlayerAction.Attack, target);
            playerActionsQueue.Enqueue(action);
            Debug.Log(currentPlayer.Stats.charName + " attacks " + target);
        }
        else
        {
            Debug.Log(currentPlayer.Stats.charName + "tried to attack " + target + " but missed!");
        }

        currentTurnOrder++;

        // Move to enemy turn after all players acted
        if (currentTurnOrder >= players.Count)
        {
            currentTurnOrder = 0;
            currentState = TurnState.EnemyChoice;
            ActionPhase();
        }
    }

    // ====== Enemy Choice ======
    private void EnemyTurn()
    {
        Debug.Log("===== Enemy turn ======");

        enemyActionsQueue.Clear();

        // Dead list + increas to players who are alive so more chance to get hit
        List<Unit> deadList = new List<Unit>();
        foreach (var player in players)
        {
            deadList.Add(player);
            if (!player.IsDead()) deadList.Add(player);
        }

        // Queue enemy actions for all enemies
        foreach (var enemyUnit in enemies)
        {
            if (enemyUnit.IsDead()) continue;

            int index = Random.Range(0, deadList.Count);
            Positions targetPos = deadList[index].CurrentPositions;
            enemyActionsQueue.Enqueue(new PlayerActionType(PlayerAction.Attack, targetPos));

            Debug.Log("Enemy will attack " + targetPos);
        }
    }


    // ====== Resolve Actions ======
    private void ActionPhase()
    {
        Debug.Log("=== Action phase ===");

        // Player actions
        foreach (var action in playerActionsQueue)
        {
            bool hit = UnityEngine.Random.value <= 0.8f;
            if (hit)
            {
                // Target enemy at the selected position
                Unit target = enemies.Find(enemy => enemy.CurrentPositions == action.TargetPos && !enemy.IsDead());
                if (target != null)
                {
                    target.TakeDamage(20);
                    Debug.Log("Player attacks " + target.Stats.charName + " at " + target.CurrentPositions);
                }
            }
            else
            {
                Unit target = enemies.Find(enemy => enemy.CurrentPositions == action.TargetPos && !enemy.IsDead());
                if (target != null)
                {
                    Debug.Log("Player missed " + target.Stats.charName);
                }
            }
        }

        // Enemy actions
        foreach (var action in enemyActionsQueue)
        {
            Unit target = players.Find(player => player.CurrentPositions == action.TargetPos && !player.IsDead());
            if (target != null)
            {
                bool hit = UnityEngine.Random.value <= 0.8f;
                if (hit)
                {
                    target.TakeDamage(15); // test enemy damage
                    Debug.Log("Enemy attacks " + target.Stats.charName + " at " + target.CurrentPositions);
                }
                else
                {
                    Debug.Log("Enemy missed " + target.Stats.charName);
                }
            }
        }

        // Clear queues
        playerActionsQueue.Clear();
        enemyActionsQueue.Clear();

        // Win/Loss checks
        if (players.TrueForAll(player => player.IsDead()))
        {
            Debug.Log("All players dead! Game Over!");
            return;
        }

        if (enemies.TrueForAll(enemy => enemy.IsDead()))
        {
            Debug.Log("All enemies defeated! Players win!");
            return;
        }

        // Start next turn
        PreviewTurn();
    }

    // ====== Update loop ======
    private void Update()
    {
        if (currentState == TurnState.Resolve)
        {
            PreviewTurn();
        }
    }

    #endregion
}
