using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnOrder : MonoBehaviour
{
    #region Unit / Character
    public enum Positions { Front, Middle, Back }

    public class Unit
    {
        public CharactersStats Stats;
        public int CurrentVigor;
        public Positions CurrentPosition;
        public bool IsPlayer;
        public int Initiative;

        public List<SkillData> abilities = new List<SkillData>();

        public Unit(CharactersStats stats, Positions pos, bool isPlayer, List<SkillData> unitAbilities)
        {
            Stats = stats;
            CurrentVigor = stats.Vigor;
            CurrentPosition = pos;
            IsPlayer = isPlayer;
            abilities = unitAbilities;
        }

        public bool IsDead() => CurrentVigor <= 0;

        public void TakeDamage(int damage)
        {
            if (IsDead()) return;
            int actualDamage = Mathf.Max(damage - Stats.Endurance, 0);
            CurrentVigor -= actualDamage;
            Debug.Log(Stats.charName + " took -" + actualDamage + " damage. Vigor =" + CurrentVigor);
        }
    }
    #endregion

    #region Player Actions
    public enum PlayerActionTypeEnum { Ability, Move }

    public class PlayerActionType
    {
        public PlayerActionTypeEnum ActionType;
        public SkillData ChosenAbility;
        public Positions TargetPos;
        public Unit Actor; // <-- store who performs this action

        public PlayerActionType(SkillData ability, TurnOrder.Positions target, TurnOrder.Unit actor)
        {
            ActionType = PlayerActionTypeEnum.Ability;
            ChosenAbility = ability;
            TargetPos = target;
            Actor = actor;
        }

        public PlayerActionType(TurnOrder.Positions moveTo, TurnOrder.Unit actor)
        {
            ActionType = PlayerActionTypeEnum.Move;
            TargetPos = moveTo;
            Actor = actor;
        }
    }

    #endregion

    #region Unit References
    [HideInInspector] public List<Unit> players = new List<Unit>();
    [HideInInspector] public List<Unit> enemies = new List<Unit>();
    [HideInInspector] public List<Unit> initiativeOrderList = new List<Unit>();

    [HideInInspector] public Queue<PlayerActionType> playerActionsQueue = new Queue<PlayerActionType>();
    [HideInInspector] public Queue<PlayerActionType> enemyActionsQueue = new Queue<PlayerActionType>();
    #endregion

    #region FSM
    private CoreGameplayLoop currentState;
    #endregion

    private void Start()
    {
        // Example: Add units here (assign SOs from Inspector)
        // players.Add(new Unit(MageStats, Positions.Back, true, new List<SkillData>{ FireballSO, RejuvenateSO, ArcaneBlastSO }));
        // enemies.Add(new Unit(GoblinStats, Positions.Front, false, new List<SkillData>{ SlashSO, StabSO, TauntSO }));

        PreviewTurn();
        currentState = new EnemyChoiceState(this);
        currentState.Enter();
    }

    public void Update() => currentState.Update();

    public void ChangeState(CoreGameplayLoop newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

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
        string turnOrderDisplay = string.Join(" -> ",
            initiativeOrderList.Select(unit => unit.Stats.charName + (unit.IsDead() ? "(Dead)" : "")));
        Debug.Log(turnOrderDisplay);
    }

    public void PlayerTurnInput(int targetPosition, SkillData chosenSkill, Unit player)
    {
        if (currentState is PlayerChoiceState playerState)
            playerState.PlayerUseSkill(player, chosenSkill);
    }
}
