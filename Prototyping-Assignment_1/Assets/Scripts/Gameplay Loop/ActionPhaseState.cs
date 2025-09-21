using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ====== Action Phase State ======
// Executes queued actions for players and enemies
// Handles damage, misses, and checks for dead units
public class ActionPhaseState : CoreGameplayLoop
{
    public ActionPhaseState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        Debug.Log("=== Action phase ===" );

        // ====== Resolve Player Actions ======
        ResolveQueue(CoreGameState.playerActionsQueue, CoreGameState.enemies, 20);

        // ====== Resolve Enemy Actions ======
        ResolveQueue(CoreGameState.enemyActionsQueue, CoreGameState.players, 15);

        
        CoreGameState.playerActionsQueue.Clear();
        CoreGameState.enemyActionsQueue.Clear();

        // ====== Check Victory / Defeat ======
        if (CoreGameState.players.All(player => player.IsDead()))
        {
            Debug.Log("All players dead! Game Over!");
            return;
        }

        if (CoreGameState.enemies.All(enemy => enemy.IsDead()))
        {
            Debug.Log("All enemies defeated! Players win!");
            return;
        }

        // Prepare for next turn
        CoreGameState.PreviewTurn();
        CoreGameState.ChangeState(new EnemyChoiceState(CoreGameState));
    }

    public override void Update() { }

    public override void Exit() { }

    // ====== End Phase Queue ======
    // Resolves actions from a queue against a list of target units
    private void ResolveQueue(Queue<TurnOrder.PlayerActionType> queue, List<TurnOrder.Unit> targets, int damage)
    {
        foreach (var action in queue)
        {
            string attackerType;
            if (queue == CoreGameState.playerActionsQueue)
                attackerType = "Player";
            else
                attackerType = "Enemy";

            // Find target that matches the chosen position and is alive
            TurnOrder.Unit target = targets.Find(unit => unit.CurrentPosition == action.TargetPos && !unit.IsDead());
            if (target == null) continue;

            // This is where we will check the Hit/Miss %
            bool hit = Random.value <= 0.8f;
            if (hit)
            {
                target.TakeDamage(damage);
                Debug.Log(attackerType + " attacks " + target.Stats.charName + " at " + target.CurrentPosition);
            }
            else
            {
                Debug.Log(attackerType + " missed " + target.Stats.charName + " at " + target.CurrentPosition);
            }
        }
    }
}
