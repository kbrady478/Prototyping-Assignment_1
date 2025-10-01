using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine;

public class TurnOrder : MonoBehaviour
{
    #region UI
    public MovingPositions movingPositions;


    public delegate void PlayerTurnStartHandler(Unit player);
    public event PlayerTurnStartHandler OnPlayerTurnStart;

    public void TriggerPlayerTurnStart(Unit player)
    {
        OnPlayerTurnStart?.Invoke(player);
    }
    #endregion

    #region Unit / Character
    public enum Positions { Front = 0, Middle = 1, Back = 2 }

    public class Unit
    {
        public CharactersStats Stats;
        public int CurrentVigor;
        public Positions CurrentPosition;
        public bool IsPlayer;
        public int Initiative;

        // For Parry/Guard skills
        public bool HasParry = false;
        public bool HasGuard = false;

        public Transform visualTransform;

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
            int actualDamage = Mathf.Max(damage - Stats.Endurance, 0);
            CurrentVigor -= actualDamage;
        }
    }
    #endregion

    #region Player Actions
    public enum PlayerActionTypeEnum { Ability, Move }

    public class PlayerActionType
    {
        public PlayerActionTypeEnum ActionType;
        public Unit User;
        public SkillData ChosenAbility;

        public PlayerActionType(Unit user, SkillData ability)
        {
            User = user;
            ChosenAbility = ability;
            ActionType = PlayerActionTypeEnum.Ability;
        }
    }
    #endregion

    #region Unit References
    public CharactersStats Mage, Gunslinger, Prince, Goblin, Goblin2, Goblin3;

    [HideInInspector] public List<Unit> players = new List<Unit>();
    [HideInInspector] public List<Unit> enemies = new List<Unit>();
    [HideInInspector] public List<Unit> initiativeOrderList = new List<Unit>();

    [HideInInspector] public Queue<PlayerActionType> playerActionsQueue = new Queue<PlayerActionType>();
    [HideInInspector] public Queue<PlayerActionType> enemyActionsQueue = new Queue<PlayerActionType>();
    #endregion

    #region FSM
    private CoreGameplayLoop currentState;
    public CoreGameplayLoop CurrentState => currentState;
    #endregion

    #region Start / Update
    [SerializeField] private GameObject healthBarPrefab;

    private void Start()
    {
        // Player Side
        players.Add(new Unit(Mage, Positions.Back, true));
        players.Add(new Unit(Gunslinger, Positions.Middle, true));
        players.Add(new Unit(Prince, Positions.Front, true));

        // Enemy Side
        enemies.Add(new Unit(Goblin, Positions.Front, false));
        enemies.Add(new Unit(Goblin2, Positions.Middle, false));
        enemies.Add(new Unit(Goblin3, Positions.Back, false));

        
        for (int i = 0; i < players.Count; i++)
            players[i].visualTransform = movingPositions.playerObjects[i].transform;

        for (int i = 0; i < enemies.Count; i++)
            enemies[i].visualTransform = movingPositions.enemyObjects[i].transform;

        // Spawn health bars 
        foreach (var unit in players.Concat(enemies))
        {
            GameObject healthUi = Instantiate(healthBarPrefab);
            HealthBar hb = healthUi.GetComponent<HealthBar>();
            hb.Initialize(unit);

            healthUi.transform.SetParent(unit.visualTransform, false);
            healthUi.transform.localPosition = new Vector3(0, 1f, 0); 
        }



        StartCoroutine(PreviewTurnRoutine());

        currentState = new EnemyChoiceState(this);
        currentState.Enter();
    }



    public void Update() => currentState.Update();
    #endregion

    #region FSM Control
    public void ChangeState(CoreGameplayLoop newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    #endregion

    #region Preview Turn
    public TurnDisplay turnDisplay; 

    public void PreviewTurn()
    {
        Debug.Log("===== Preview turn =====");

        initiativeOrderList.Clear();
        initiativeOrderList.AddRange(players);
        initiativeOrderList.AddRange(enemies);

        foreach (var unit in initiativeOrderList)
        {
            int roll = Random.Range(0, 20);
            unit.Initiative = Mathf.Max(roll + Mathf.RoundToInt(unit.Stats.Agility));
        }

        initiativeOrderList = initiativeOrderList.OrderByDescending(u => u.Initiative).ToList();

        string display = string.Join(" -> ",
            initiativeOrderList.Select(u => u.Stats.charName + (u.IsDead() ? "(Dead)" : "")));
        Debug.Log(display);
    }

    public IEnumerator PreviewTurnRoutine()
    {
        Debug.Log("===== Preview turn =====");

        initiativeOrderList.Clear();
        initiativeOrderList.AddRange(players);
        initiativeOrderList.AddRange(enemies);

        foreach (var unit in initiativeOrderList)
        {
            int roll = Random.Range(0, 20);
            unit.Initiative = Mathf.Max(roll + Mathf.RoundToInt(unit.Stats.Agility), 0); 
        }

        initiativeOrderList = initiativeOrderList.OrderByDescending(u => u.Initiative).ToList();

        string display = string.Join(" -> ",
            initiativeOrderList.Select(u => u.Stats.charName + (u.IsDead() ? "(Dead)" : "")));
        Debug.Log(display);

        if (turnDisplay != null)
            turnDisplay.UpdateTurnOrderDisplay();


        yield return null; // allow coroutine to yield
    }

    #endregion

    #region Player Turns
    
    private int currentPlayerIndex = -1; 

    public IEnumerator NextAlivePlayerTurn()
    {
        if (players.All(p => p.IsDead()))
            yield break;

        int totalPlayers = initiativeOrderList.Count;
        int iterations = 0;

        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
            iterations++;

            var candidate = initiativeOrderList[currentPlayerIndex];
            if (candidate.IsPlayer && !candidate.IsDead())
            {
                Debug.Log("It's " + candidate.Stats.charName + "'s turn!");
                yield return new WaitForSeconds(1.5f);
                OnPlayerTurnStart?.Invoke(candidate);
                yield break;
            }

        } while (iterations <= totalPlayers);
    }

    #endregion

    #region Positional/Status Checks

    public IEnumerator DisplayAllUnitStatus(float displayTime = 2f)
    {
       Debug.Log("===== Unit Status =====");
        
        foreach (var unit in initiativeOrderList)
        {
            string status = unit.IsDead()
                ? "(Dead)"
                : unit.CurrentVigor + "/" + unit.Stats.Vigor;

            string position = unit.CurrentPosition.ToString();

            Debug.Log(unit.Stats.charName + ": " + status + " | Position: " + position);
        }

        yield return new WaitForSeconds(displayTime); 
    }

    public bool HasPlayerChosen(Unit player)
    {
        return playerActionsQueue.Any(action => action.User == player);
    }
    #endregion
}
