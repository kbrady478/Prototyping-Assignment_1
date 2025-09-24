using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ActionPhaseState : CoreGameplayLoop
{
    public ActionPhaseState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        Debug.Log("=== Action phase ===");

        ResolveQueue(CoreGameState.playerActionsQueue, CoreGameState.enemies, CoreGameState.players);
        ResolveQueue(CoreGameState.enemyActionsQueue, CoreGameState.players, CoreGameState.enemies);

        CoreGameState.playerActionsQueue.Clear();
        CoreGameState.enemyActionsQueue.Clear();

        if (CoreGameState.players.All(p => p.IsDead()))
        {
            Debug.Log("All players dead! Game Over!");
            return;
        }

        if (CoreGameState.enemies.All(e => e.IsDead()))
        {
            Debug.Log("All enemies defeated! Players win!");
            return;
        }

        CoreGameState.PreviewTurn();
        CoreGameState.ChangeState(new EnemyChoiceState(CoreGameState));
    }

    public override void Update() { }
    public override void Exit() { }

    private void ResolveQueue(Queue<TurnOrder.PlayerActionType> queue, List<TurnOrder.Unit> targets, List<TurnOrder.Unit> allUnits)
    {
        foreach (var action in queue)
        {
            string attackerType = queue == CoreGameState.playerActionsQueue ? "Player" : "Enemy";
            TurnOrder.Unit unit = action.Actor; 


            if (action.ActionType == TurnOrder.PlayerActionTypeEnum.Move)
            {
                if (unit != null)
                {
                    unit.CurrentPosition = action.TargetPos;
                    Debug.Log(attackerType + " moves to " + action.TargetPos);
                }
                continue;
            }

            // Resolve skill
            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                var unitsInPosition = targets.Where(u => u.CurrentPosition == targetPos && !u.IsDead()).ToList();
                foreach (var target in unitsInPosition)
                {
                    bool hit = Random.value <= action.ChosenAbility.hitChance;
                    if (!hit) { Debug.Log(attackerType + " missed " + target.Stats.charName); continue; }

                    if (action.ChosenAbility.isHeal)
                    {
                        target.CurrentVigor += action.ChosenAbility.damage;
                        Debug.Log(attackerType + " heals " + target.Stats.charName + " for " + action.ChosenAbility.damage);
                    }
                    else if (action.ChosenAbility.isBuff)
                    {
                        Debug.Log(attackerType + " uses " + action.ChosenAbility.skillName + " on " + target.Stats.charName);
                    }
                    else
                    {
                        target.TakeDamage(action.ChosenAbility.damage);
                        Debug.Log(attackerType + " uses " + action.ChosenAbility.skillName + " hitting " + target.Stats.charName);
                    }
                }
            }

            // Move the acting unit
            if (unit != null && action.ChosenAbility != null)
            {
                int newPosIndex = (int)unit.CurrentPosition + action.ChosenAbility.MoveSelf;
                newPosIndex = Mathf.Clamp(newPosIndex, 0, 2);
                unit.CurrentPosition = (TurnOrder.Positions)newPosIndex;
                Debug.Log(attackerType + " moves to " + unit.CurrentPosition + " after skill");
            }

            // Apply skill effects to targets
            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                var unitsInPosition = targets.Where(u => u.CurrentPosition == targetPos && !u.IsDead()).ToList();

                foreach (var target in unitsInPosition)
                {
                    // Hit check
                    bool hit = Random.value <= action.ChosenAbility.hitChance;
                    if (!hit) { Debug.Log(attackerType + " missed " + target.Stats.charName); continue; }

                    // Damage / Heal / Buff
                    if (action.ChosenAbility.isHeal)
                    {
                        target.CurrentVigor += action.ChosenAbility.damage;
                        Debug.Log(attackerType + " heals " + target.Stats.charName + " for " + action.ChosenAbility.damage);
                    }
                    else if (action.ChosenAbility.isBuff)
                    {
                        Debug.Log(attackerType + " uses buff " + action.ChosenAbility.skillName + " on " + target.Stats.charName);
                    }
                    else
                    {
                        target.TakeDamage(action.ChosenAbility.damage);
                        Debug.Log(attackerType + " hits " + target.Stats.charName + " with " + action.ChosenAbility.skillName);
                    }

                    // Move target if skill has relativeMoveTarget
                    int targetNewIndex = (int)target.CurrentPosition + action.ChosenAbility.MoveTarget;
                    targetNewIndex = Mathf.Clamp(targetNewIndex, 0, 2);
                    target.CurrentPosition = (TurnOrder.Positions)targetNewIndex;
                    if (action.ChosenAbility.MoveTarget != 0)
                        Debug.Log(target.Stats.charName + " is pushed/moved to " + target.CurrentPosition);
                }
            }


        }
    }
}
