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

        foreach (var enemy in CoreGameState.enemies)
        {
            if (enemy.IsDead()) continue;

            // Pick random skill
            SkillData chosenSkill = enemy.abilities[Random.Range(0, enemy.abilities.Count)];

            // Pick target position from skill
            TurnOrder.Positions targetPos = chosenSkill.targetPositions[Random.Range(0, chosenSkill.targetPositions.Length)];

            CoreGameState.enemyActionsQueue.Enqueue(new TurnOrder.PlayerActionType(chosenSkill, targetPos, enemy));

            Debug.Log(enemy.Stats.charName + " will use " + chosenSkill.skillName + " on " + targetPos);
        }

        CoreGameState.ChangeState(new PlayerChoiceState(CoreGameState));
    }

    public override void Update() { }
    public override void Exit() { }
}
