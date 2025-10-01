using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActionPhaseState : CoreGameplayLoop
{
    public ActionPhaseState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        CoreGameState.StartCoroutine(ActionPhaseRoutine());
    }

    public override void Update() { }

    public override void Exit() { }

    private IEnumerator ActionPhaseRoutine()
    {
        Debug.Log("=== Action phase ===");
        yield return new WaitForSeconds(1f);

        // Loop through units in initiative order
        foreach (var unit in CoreGameState.initiativeOrderList)
        {
            // Do NOT skip dead units here — they still occupy positions
            if (unit.IsPlayer)
            {
                var playerAction = CoreGameState.playerActionsQueue.FirstOrDefault(a => a.User == unit);
                if (playerAction != null)
                {
                    var singleActionQueue = new Queue<TurnOrder.PlayerActionType>();
                    singleActionQueue.Enqueue(playerAction);

                    yield return ResolveQueue(singleActionQueue, CoreGameState.enemies, CoreGameState.players);
                    CoreGameState.playerActionsQueue = new Queue<TurnOrder.PlayerActionType>(
                        CoreGameState.playerActionsQueue.Where(a => a.User != unit)
                    );
                }
            }
            else
            {
                var enemyAction = CoreGameState.enemyActionsQueue.FirstOrDefault(a => a.User == unit);
                if (enemyAction != null)
                {
                    var singleActionQueue = new Queue<TurnOrder.PlayerActionType>();
                    singleActionQueue.Enqueue(enemyAction);

                    yield return ResolveQueue(singleActionQueue, CoreGameState.players, CoreGameState.enemies);
                    CoreGameState.enemyActionsQueue = new Queue<TurnOrder.PlayerActionType>(
                        CoreGameState.enemyActionsQueue.Where(a => a.User != unit)
                    );
                }
            }

            yield return new WaitForSeconds(1f);
        }

        // Display status before next round
        yield return CoreGameState.StartCoroutine(CoreGameState.DisplayAllUnitStatus(2f));

        // Reset for next round
        CoreGameState.currentPlayerIndex = -1;
        CoreGameState.playerActionsQueue.Clear();
        CoreGameState.enemyActionsQueue.Clear();

        // End-of-round checks
        if (CoreGameState.players.All(p => p.IsDead()))
        {
            Debug.Log("All players dead! Game Over!");
            yield break;
        }

        if (CoreGameState.enemies.All(e => e.IsDead()))
        {
            Debug.Log("All enemies defeated! Players win!");
            yield break;
        }

        // Roll new initiatives for the next round
        yield return CoreGameState.StartCoroutine(CoreGameState.PreviewTurnRoutine());

        // Next turn
        CoreGameState.ChangeState(new EnemyChoiceState(CoreGameState));
    }

    private IEnumerator ResolveQueue(Queue<TurnOrder.PlayerActionType> queue, List<TurnOrder.Unit> targets, List<TurnOrder.Unit> allUnits)
    {
        foreach (var action in queue)
        {
            var unit = action.User;
            if (unit == null || action.ChosenAbility == null)
                continue;

            yield return new WaitForSeconds(1f);

            // Move self
            if (action.ChosenAbility.MoveSelf != 0)
            {
                int newUnitIndex = Mathf.Clamp((int)unit.CurrentPosition + action.ChosenAbility.MoveSelf, 0, 2);
                Move(unit, allUnits, newUnitIndex);
                
                yield return new WaitForSeconds(1f);
            }

            // Healing
            if (action.ChosenAbility.isHeal)
            {
                var team = unit.IsPlayer ? CoreGameState.players : CoreGameState.enemies;

                foreach (var member in team)
                {
                    if (member.IsDead()) continue;

                    int healAmount = (unit.CurrentPosition != TurnOrder.Positions.Back) ? 10 : action.ChosenAbility.damage;
                    member.CurrentVigor = Mathf.Min(member.CurrentVigor + healAmount, member.Stats.Vigor);

                    Debug.Log(unit.Stats.charName + " heals " + member.Stats.charName + " for " + healAmount);
                }

                continue; // skip damage/buff logic
            }

            // Process non-heal abilities
            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                // Include dead units so positions are always occupied
                var unitsInPos = targets.Where(u => u.CurrentPosition == targetPos).ToList();

                foreach (var target in unitsInPos)
                {
                    if (!target.IsDead())
                    {
                        bool hit = Random.value <= action.ChosenAbility.hitChance;
                        if (!hit)
                        {
                            Debug.Log(unit.Stats.charName + " missed " + target.Stats.charName);
                            continue;
                        }

                        if (action.ChosenAbility.isStatus)
                        {
                            if (action.ChosenAbility.skillName.ToLower().Contains("parry"))
                            {
                                target.HasParry = true;
                                Debug.Log(target.Stats.charName + " activates Parry!");
                            }
                            else if (action.ChosenAbility.skillName.ToLower().Contains("en guard"))
                            {
                                target.HasGuard = true;
                                Debug.Log(target.Stats.charName + " is Guarding!");
                            }
                        }
                        else
                        {
                            if (target.HasGuard)
                            {
                                int dmg = Mathf.RoundToInt(action.ChosenAbility.damage * 0.5f);
                                target.CurrentVigor -= dmg;
                                target.HasGuard = false;
                                Debug.Log(target.Stats.charName + " guards! Damage reduced to " + dmg);
                            }
                            else if (target.HasParry)
                            {
                                int refl = Mathf.RoundToInt(action.ChosenAbility.damage * 0.5f);
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
                    }

                    // Move target regardless of alive/dead
                    if (action.ChosenAbility.MoveTarget != 0)
                    {
                        int targetIndex = Mathf.Clamp((int)target.CurrentPosition + action.ChosenAbility.MoveTarget, 0, 2);
                        Move(target, targets, targetIndex);
                        yield return new WaitForSeconds(1f);
                    }
                }
            }
        }
    }

    private void Move(TurnOrder.Unit unit, List<TurnOrder.Unit> allUnits, int newPos)
    {
        var occupiedUnit = allUnits.FirstOrDefault(u => u != unit && u.CurrentPosition == (TurnOrder.Positions)newPos);

        if (occupiedUnit != null)
        {
            int oldPos = (int)unit.CurrentPosition;
            unit.CurrentPosition = (TurnOrder.Positions)newPos;
            occupiedUnit.CurrentPosition = (TurnOrder.Positions)oldPos;

            // Update visuals
            if (unit.IsPlayer)
                CoreGameState.movingPositions.playerObjects[allUnits.IndexOf(unit)].transform.position =
                    CoreGameState.movingPositions.playerPositions[newPos].position;
            else
                CoreGameState.movingPositions.enemyObjects[allUnits.IndexOf(unit)].transform.position =
                    CoreGameState.movingPositions.enemyPositions[newPos].position;

            if (occupiedUnit.IsPlayer)
                CoreGameState.movingPositions.playerObjects[allUnits.IndexOf(occupiedUnit)].transform.position =
                    CoreGameState.movingPositions.playerPositions[oldPos].position;
            else
                CoreGameState.movingPositions.enemyObjects[allUnits.IndexOf(occupiedUnit)].transform.position =
                    CoreGameState.movingPositions.enemyPositions[oldPos].position;
        }
        else
        {
            unit.CurrentPosition = (TurnOrder.Positions)newPos;

            if (unit.IsPlayer)
                CoreGameState.movingPositions.playerObjects[allUnits.IndexOf(unit)].transform.position =
                    CoreGameState.movingPositions.playerPositions[newPos].position;
            else
                CoreGameState.movingPositions.enemyObjects[allUnits.IndexOf(unit)].transform.position =
                    CoreGameState.movingPositions.enemyPositions[newPos].position;
        }

        Debug.Log(unit.Stats.charName + " moved to " + unit.CurrentPosition);
    }
}
