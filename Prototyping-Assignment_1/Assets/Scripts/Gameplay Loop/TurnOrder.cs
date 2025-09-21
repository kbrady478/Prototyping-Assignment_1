using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ====== TurnOrder Controller ======
// Handles the turn order, units, queues, and state machine
// Runs whose turn it is using a state machine
public class TurnOrder : MonoBehaviour
{
    #region Unit / Character
    // ====== UNIT / CHARACTER =======
    // Here Should track said units data (character stats, Positions, state Player || Enemy )
    // Remember Dead positions, IF Miss.... return otherwise do said actions
    public enum Positions { Front, Middle, Back }

    public class Unit
    {

        public CharactersStats Stats;
        public int CurrentVigor;
        public Positions CurrentPosition;
        public bool IsPlayer;
        public int Initiative;

        public Unit(CharactersStats stats, Positions pos, bool isPlayer)
        {
            Stats = stats;
            CurrentVigor = stats.Vigor;
            CurrentPosition = pos;
            IsPlayer = isPlayer;
        }

        public bool IsDead() => CurrentVigor <= 0;

        public void TakeDamage(int damage)
        {
            if (IsDead()) return;

            // Damage check if lower than Endurance
            int actualDamage = Mathf.Max(damage - Stats.Endurance, 0);
            CurrentVigor -= actualDamage;

            Debug.Log(Stats.charName + " took -" + actualDamage + " damage. Vigor =" + CurrentVigor);
        }
    }
    #endregion

    #region Player Actions
    // ====== Player Actions =======
    // Should control the action the player does using buttons in the UI
    // Resolves said actions in a waiting queue list
    public enum PlayerAction { Attack }

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

    #region Unit References
    // ====== Scriptable Objects ======
    // Grab from the instance variable from the SO scripts
    public CharactersStats Mage, Gunslinger, Prince, Goblin, Goblin2, Goblin3;

    // Lists
    [HideInInspector] public List<Unit> players = new List<Unit>();
    [HideInInspector] public List<Unit> enemies = new List<Unit>();
    [HideInInspector] public List<Unit> initiativeOrderList = new List<Unit>();

    [HideInInspector] public Queue<PlayerActionType> playerActionsQueue = new Queue<PlayerActionType>();
    [HideInInspector] public Queue<PlayerActionType> enemyActionsQueue = new Queue<PlayerActionType>();
    #endregion

    #region FSM
    private CoreGameplayLoop currentState;
    #endregion

    #region Start / Update
    private void Start()
    {
        // ====== Starting Order =======
        // BACK MIDDLE FRONT FRONT MIDDLE BACK
        // MAGE GUNSL  PRIN  GOB   GOB    GOB
        // ENEMY > PLAYER > ACTION

        // Player Side
        players.Add(new Unit(Mage, Positions.Back, true));
        players.Add(new Unit(Gunslinger, Positions.Middle, true));
        players.Add(new Unit(Prince, Positions.Front, true));

        // Enemy Side
        enemies.Add(new Unit(Goblin, Positions.Front, false));
        enemies.Add(new Unit(Goblin2, Positions.Middle, false));
        enemies.Add(new Unit(Goblin3, Positions.Back, false));

        // Shows the current what happens
        PreviewTurn();

        // Start FSM
        currentState = new EnemyChoiceState(this);
        currentState.Enter();
    }

    public void Update() => currentState.Update();
    #endregion

    #region FSM Control
    // ====== Change State ======
    // State Switching
    public void ChangeState(CoreGameplayLoop newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    #endregion

    #region Preview Turn
    // ====== Preview Turn Order =====
    // Players must be able to see who goes in what order
    // See what the enemy will do in their turn
    // Then players will decide what they will do
    public void PreviewTurn()
    {
        Debug.Log("===== Preview turn =====");

        initiativeOrderList.Clear();
        initiativeOrderList.AddRange(players);
        initiativeOrderList.AddRange(enemies);

        // Roll initiative for each unit
        foreach (var unit in initiativeOrderList)
        {
            // D20 dice rolls
            int roll = Random.Range(0, 20);
            unit.Initiative = Mathf.Max(roll + Mathf.RoundToInt(unit.Stats.Agility));
        }

        initiativeOrderList = initiativeOrderList.OrderByDescending(unit => unit.Initiative).ToList();

        string turnOrderDisplay = string.Join(" -> ",
            initiativeOrderList.Select(unit => unit.Stats.charName + (unit.IsDead() ? "(Dead)" : "")));
        Debug.Log(turnOrderDisplay);
    }
    #endregion

    #region Player Input
    // ====== UI buttons ======
    public void PlayerTurnInput(int targetPosition)
    {
        if (currentState is PlayerChoiceState playerState)
            playerState.PlayerTurn(targetPosition);
    }
    #endregion
}
