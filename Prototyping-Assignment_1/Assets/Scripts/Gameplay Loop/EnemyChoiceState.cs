using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyChoiceState : CoreGameplayLoop
{
    public EnemyChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        Debug.Log("===== Enemy Choice Phase =====");
        CoreGameState.enemyActionsQueue.Clear();

        List<TurnOrder.Unit> alivePlayers = CoreGameState.players.Where(p => !p.IsDead()).ToList();
        if (alivePlayers.Count == 0) return;

        foreach (var enemy in CoreGameState.enemies)
        {
            if (enemy.IsDead()) continue;

            SkillData[] enemySkills = enemy.Stats.skills;
            SkillData chosenSkill = enemySkills[Random.Range(0, enemySkills.Length)];

            CoreGameState.enemyActionsQueue.Enqueue(
                new TurnOrder.PlayerActionType(enemy, chosenSkill)
            );

            Debug.Log(enemy.Stats.charName + " will use " + chosenSkill.skillName);
        }

        CoreGameState.ChangeState(new ActionPhaseState(CoreGameState));
    }

    public override void Update() { }
    public override void Exit() { }
}
