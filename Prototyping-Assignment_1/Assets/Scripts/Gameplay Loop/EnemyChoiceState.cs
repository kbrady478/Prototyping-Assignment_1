using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyChoiceState : CoreGameplayLoop
{
    public EnemyChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        CoreGameState.StartCoroutine(EnemyChoiceRoutine());
    }

    public override void Update() { }
    public override void Exit() { }

    private IEnumerator EnemyChoiceRoutine()
    {
        Debug.Log("===== Enemy Choice Phase =====");

        CoreGameState.enemyActionsQueue.Clear();

        // Only consider alive enemies
        List<TurnOrder.Unit> aliveEnemies = CoreGameState.enemies.Where(e => !e.IsDead()).ToList();

        foreach (var enemy in aliveEnemies)
        {
            SkillData[] enemySkills = enemy.Stats.skills;
            SkillData chosenSkill = enemySkills[Random.Range(0, enemySkills.Length)];

            CoreGameState.enemyActionsQueue.Enqueue(new TurnOrder.PlayerActionType(enemy, chosenSkill));
            Debug.Log(enemy.Stats.charName + " will use " + chosenSkill.skillName);

            yield return new WaitForSeconds(1f); // pacing per enemy decision
        }

        // After all enemies have queued actions, go to action phase
        CoreGameState.ChangeState(new ActionPhaseState(CoreGameState));
    }
}
