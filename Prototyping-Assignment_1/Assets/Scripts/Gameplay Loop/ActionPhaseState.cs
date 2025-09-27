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

            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                // ---------- new line of code ----------
                TurnOrder.Unit target = null; // ---------- new line of code ----------
                if (action.ChosenAbility.skillName == "Magic Missile" && unit.IsPlayer) // ---------- new line of code ----------
                { // ---------- new line of code ----------
                    target = targets.FirstOrDefault(u => u.CurrentPosition == unit.CurrentPosition && !u.IsDead()); // ---------- new line of code ----------
                } // ---------- new line of code ----------
                else // ---------- new line of code ----------
                { // ---------- new line of code ----------
                    target = targets.FirstOrDefault(u => u.CurrentPosition == targetPos && !u.IsDead()); // ---------- new line of code ----------
                } // ---------- new line of code ----------

                if (target != null)
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
                            int healAmount = action.ChosenAbility.damage; // ---------- new line of code ----------
                            if (unit.IsPlayer) // ---------- new line of code ----------
                            { // ---------- new line of code ----------
                                if (unit.Stats.charName == "Mage" && unit.CurrentPosition == TurnOrder.Positions.Back) // ---------- new line of code ----------
                                    healAmount = 20; // ---------- new line of code ----------
                                else // ---------- new line of code ----------
                                    healAmount = 10; // ---------- new line of code ----------
                            } // ---------- new line of code ----------
                            else // Enemy heals themselves only // ---------- new line of code ----------
                            { // ---------- new line of code ----------
                                healAmount = action.ChosenAbility.damage; // ---------- new line of code ----------
                            } // ---------- new line of code ----------

                            target.CurrentVigor += healAmount; // ---------- new line of code ----------
                            Debug.Log(unit.Stats.charName + " heals " + target.Stats.charName + " for " + healAmount); // ---------- new line of code ----------
                        }
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
                                int dmg = Mathf.RoundToInt(action.ChosenAbility.damage * 0.5f); // Halves damage
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

                        // ----------- Movement ------------
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
                        // ----------- Movement ------------

                        yield return new WaitForSeconds(1.5f);
                    }
                }
            }
        }
    }
}
