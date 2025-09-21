using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ====== Enemy Choice State ======
// Handles the AI decision for enemies choices
// Determines which player each enemy will attack
public class EnemyChoiceState : CoreGameplayLoop
{
    public EnemyChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        Debug.Log("===== Enemy Choice Phase =====" );

        // Clears previous queue
        CoreGameState.enemyActionsQueue.Clear();

        
        // Track alive players
        List<TurnOrder.Unit> alivePlayers = CoreGameState.players.Where(player => !player.IsDead()).ToList();

        if (alivePlayers.Count == 0) return;

        // Randomly pick targets for each enemy
        foreach (var enemy in CoreGameState.enemies)
        {
            if (enemy.IsDead()) continue;

            int index = Random.Range(0, alivePlayers.Count);
            CoreGameState.enemyActionsQueue.Enqueue(
                new TurnOrder.PlayerActionType(TurnOrder.PlayerAction.Attack, alivePlayers[index].CurrentPosition)
            );

            Debug.Log(enemy.Stats.charName + " will attack " + alivePlayers[index].CurrentPosition);
        }

        // Immediately proceed to resolve phase
        CoreGameState.ChangeState(new PlayerChoiceState(CoreGameState));
    }

    public override void Update() { }

    public override void Exit() { }
}
