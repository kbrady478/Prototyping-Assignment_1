using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// RESOLVE STAGE
// HANDLES MAKING SURE ALL THE CHOICES MADE ARE DISPLAYED HERE
// CREATE CHECKS FOR NEXT LOOP
// ALL DAMAGE HAS BEEN DEALT BEFORE GOING TO THE NEXT STAGE

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
            var unit = action.User;
            if (unit == null || unit.IsDead() || action.ChosenAbility == null)
                continue;

            // Move self
            int newIndex = (int)unit.CurrentPosition + action.ChosenAbility.MoveSelf;
            newIndex = Mathf.Clamp(newIndex, 0, 2);
            unit.CurrentPosition = (TurnOrder.Positions)newIndex;
            if (action.ChosenAbility.MoveSelf != 0)
                Debug.Log(unit.Stats.charName + " moves to " + unit.CurrentPosition);

            // Apply skill to targets
            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                var unitsInPos = targets.Where(u => u.CurrentPosition == targetPos && !u.IsDead()).ToList();
                foreach (var target in unitsInPos)
                {
                    bool hit = Random.value <= action.ChosenAbility.hitChance;
                    if (!hit) { Debug.Log(unit.Stats.charName + " missed " + target.Stats.charName); continue; }

                    if (action.ChosenAbility.isHeal)
                    {
                        target.CurrentVigor += action.ChosenAbility.damage;
                        Debug.Log(unit.Stats.charName + " heals " + target.Stats.charName + " for " + action.ChosenAbility.damage);
                    }
                    else if (action.ChosenAbility.isBuff)
                    {
                        if (action.ChosenAbility.skillName.ToLower().Contains("Parry"))
                        {
                            target.HasParry = true;
                            Debug.Log(target.Stats.charName + " activates Parry!");
                        }
                        else if (action.ChosenAbility.skillName.ToLower().Contains("En Guard"))
                        {
                            target.HasGuard = true;
                            Debug.Log(target.Stats.charName + " is Guarding!");
                        }
                    }
                    else
                    {
                        // Guard
                        if (target.HasGuard)
                        {
                            int dmg = Mathf.FloorToInt(action.ChosenAbility.damage * 0.5f);
                            target.CurrentVigor -= dmg;
                            target.HasGuard = false;
                            Debug.Log(target.Stats.charName + " guards! Damage reduced to " + dmg);
                        }
                        // Parry
                        else if (target.HasParry)
                        {
                            int refl = Mathf.FloorToInt(action.ChosenAbility.damage * 0.5f);
                            unit.TakeDamage(refl);
                            target.HasParry = false;
                            Debug.Log(target.Stats.charName + " parried! Reflected " + refl + " damage to " + unit.Stats.charName);
                        }
                        else
                        {
                            target.TakeDamage(action.ChosenAbility.damage);
                            Debug.Log(unit.Stats.charName + " hits " + target.Stats.charName + " with " + action.ChosenAbility.skillName);
                        }
                    }

                    // Move target if MoveTarget != 0
                    int targetIndex = (int)target.CurrentPosition + action.ChosenAbility.MoveTarget;
                    targetIndex = Mathf.Clamp(targetIndex, 0, 2);
                    target.CurrentPosition = (TurnOrder.Positions)targetIndex;
                    if (action.ChosenAbility.MoveTarget != 0)
                        Debug.Log(target.Stats.charName + " is moved to " + target.CurrentPosition);
                }
            }
        }
    }
}
