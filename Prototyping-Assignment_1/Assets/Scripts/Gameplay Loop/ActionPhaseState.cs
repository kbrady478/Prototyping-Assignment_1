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

        foreach (var unit in CoreGameState.initiativeOrderList)
        {
            if (unit.IsDead()) continue;

            if (unit.IsPlayer)
            {
                if (CoreGameState.playerActionsQueue.Count > 0)
                {
                    yield return ResolveQueue(CoreGameState.playerActionsQueue, CoreGameState.enemies, CoreGameState.players);
                    CoreGameState.playerActionsQueue.Clear();
                }
            }
            else
            {
                var enemySkills = unit.Stats.skills;
                var chosenSkill = enemySkills[Random.Range(0, enemySkills.Length)];
                CoreGameState.enemyActionsQueue.Enqueue(new TurnOrder.PlayerActionType(unit, chosenSkill));
                Debug.Log(unit.Stats.charName + " will use " + chosenSkill.skillName);

                yield return ResolveQueue(CoreGameState.enemyActionsQueue, CoreGameState.players, CoreGameState.enemies);
                CoreGameState.enemyActionsQueue.Clear();
            }

            yield return new WaitForSeconds(1f);
        }

        CoreGameState.ResolvePositionConflicts();

        // Display Status Before Next Round Begins
        yield return CoreGameState.StartCoroutine(CoreGameState.DisplayAllUnitStatus(2f));

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

        // Next turn
        CoreGameState.ChangeState(new EnemyChoiceState(CoreGameState));
    }




    private IEnumerator ResolveQueue(Queue<TurnOrder.PlayerActionType> queue, List<TurnOrder.Unit> targets, List<TurnOrder.Unit> allUnits)
    {
        foreach (var action in queue)
        {
            var unit = action.User;
            if (unit == null || unit.IsDead() || action.ChosenAbility == null)
                continue;

            if (action.ChosenAbility.MoveSelf != 0)
                Debug.Log(unit.Stats.charName + " moves to " + unit.CurrentPosition);

            yield return new WaitForSeconds(1.5f);


            // Ability Check
            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                // Special case: Mage's Magic Missile
                List<TurnOrder.Unit> unitsInPos;
                    if (unit.IsPlayer && action.ChosenAbility.skillName.ToLower() == "magic missile")
                    {
                        // Target enemies at the same relative position as the mage
                        unitsInPos = targets
                            .Where(u => u.CurrentPosition == unit.CurrentPosition && !u.IsDead())
                            .ToList();
                    }
                    else
                    {
                        // Default targeting
                        unitsInPos = targets.Where(u => u.CurrentPosition == targetPos && !u.IsDead()).ToList();
                    }


                    foreach (var target in unitsInPos)
                    {
                    bool hit = Random.value <= action.ChosenAbility.hitChance;
                    if (!hit)
                    {
                        Debug.Log(unit.Stats.charName + " missed " + target.Stats.charName);
                    }
                    else
                    {
                        // Handle heal, buff, damage, parry, guard etc.
                        if (action.ChosenAbility.isHeal)
                        {
                            if (unit.IsPlayer)
                            {
                                // Player heal: group heal
                                int healAmount = unit.CurrentPosition == TurnOrder.Positions.Back ? 20 : 10;
                                foreach (var ally in CoreGameState.players.Where(p => !p.IsDead()))
                                {
                                    ally.CurrentVigor += healAmount;
                                    
                                    // Makes sure you can't abuse overhealing
                                    ally.CurrentVigor = Mathf.Min(ally.CurrentVigor, ally.Stats.Vigor);
                                    Debug.Log(unit.Stats.charName + " heals " + ally.Stats.charName + " for " + healAmount);
                                }
                            }
                            else
                            {
                                // Enemy heal: only self
                                target.CurrentVigor += action.ChosenAbility.damage;

                                // Makes sure you can't abuse overhealing
                                target.CurrentVigor = Mathf.Min(target.CurrentVigor, target.Stats.Vigor);
                                Debug.Log(unit.Stats.charName + " heals themselves for " + action.ChosenAbility.damage);
                            }
                        }

                        // Status
                        else if (action.ChosenAbility.isStatus)
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
                            // Handle damage, guard, parry
                            if (target.HasGuard)
                            {
                                int dmg = Mathf.RoundToInt(action.ChosenAbility.damage * 0.5f); // Halves damage recieved
                                target.CurrentVigor -= dmg;
                                target.HasGuard = false;
                                Debug.Log(target.Stats.charName + " guards! Damage reduced to " + dmg);
                            }
                            else if (target.HasParry)
                            {
                                int refl = Mathf.RoundToInt(action.ChosenAbility.damage * 0.5f); // Halves damage atk
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

                    // -----------  Movement  ------------
                    // Move self
                    int newIndex = (int)unit.CurrentPosition + action.ChosenAbility.MoveSelf;
                    newIndex = Mathf.Clamp(newIndex, 0, 2);
                    unit.CurrentPosition = (TurnOrder.Positions)newIndex;

                    // Resolve conflicts
                    unit.CurrentPosition = CoreGameState.ResolveOccupiedPosition(unit);

                    // Move target if MoveTarget != 0
                    int targetIndex = (int)target.CurrentPosition + action.ChosenAbility.MoveTarget;
                    targetIndex = Mathf.Clamp(targetIndex, 0, 2);
                    target.CurrentPosition = (TurnOrder.Positions)targetIndex;

                    // Resolve conflicts for target
                    target.CurrentPosition = CoreGameState.ResolveOccupiedPosition(target);

                    if (action.ChosenAbility.MoveTarget != 0)
                        Debug.Log(target.Stats.charName + " is moved to " + target.CurrentPosition);
                    // -----------  Movement  ------------

                    yield return new WaitForSeconds(1.5f);
                }
            }
        }
    }


}
