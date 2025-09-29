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

        List<TurnOrder.Unit> aliveEnemies = CoreGameState.enemies.Where(enemy => !enemy.IsDead()).ToList();

        EnemyDisplay display = Object.FindFirstObjectByType<EnemyDisplay>();
        if (display != null)
            display.DisplayEnemyActions(aliveEnemies);

        yield return new WaitForSeconds(aliveEnemies.Count * 1.1f);

        // Change state after displaying choices
        CoreGameState.ChangeState(new PlayerChoiceState(CoreGameState));
    }

}
