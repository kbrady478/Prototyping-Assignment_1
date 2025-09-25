using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnOrder : MonoBehaviour
{
    #region Unit / Character
    public enum Positions { Front = 0, Middle = 1, Back = 2 }

    public class Unit
    {
        public CharactersStats Stats;
        public int CurrentVigor;
        public Positions CurrentPosition;
        public bool IsPlayer;
        public int Initiative;

        // For defensive skills
        public bool HasParry = false;
        public bool HasGuard = false;

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
            Debug.Log(Stats.charName + " took " + actualDamage + " damage. Vigor=" + CurrentVigor);
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
    #endregion

    #region Start / Update
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

        PreviewTurn();

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
    #endregion
}
