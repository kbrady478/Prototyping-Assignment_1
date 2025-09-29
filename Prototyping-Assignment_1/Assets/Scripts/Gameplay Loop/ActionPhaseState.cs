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
            if (unit.IsDead()) continue;

            if (unit.IsPlayer)
            {
                // Check for queued player action
                var playerAction = CoreGameState.playerActionsQueue.FirstOrDefault(a => a.User == unit);
                if (playerAction != null)
                {
                    Queue<TurnOrder.PlayerActionType> singleActionQueue = new Queue<TurnOrder.PlayerActionType>();
                    singleActionQueue.Enqueue(playerAction);

                    yield return ResolveQueue(singleActionQueue, CoreGameState.enemies, CoreGameState.players);
                    CoreGameState.playerActionsQueue = new Queue<TurnOrder.PlayerActionType>(
                        CoreGameState.playerActionsQueue.Where(a => a.User != unit)
                    );
                }
            }
            else
            {
                // Check for queued enemy action
                var enemyAction = CoreGameState.enemyActionsQueue.FirstOrDefault(a => a.User == unit);
                if (enemyAction != null)
                {
                    Queue<TurnOrder.PlayerActionType> singleActionQueue = new Queue<TurnOrder.PlayerActionType>();
                    singleActionQueue.Enqueue(enemyAction);

                    yield return ResolveQueue(singleActionQueue, CoreGameState.players, CoreGameState.enemies);
                    CoreGameState.enemyActionsQueue = new Queue<TurnOrder.PlayerActionType>(
                        CoreGameState.enemyActionsQueue.Where(a => a.User != unit)
                    );
                }
            }

            yield return new WaitForSeconds(1f);
        }

        // Display Status Before Next Round Begins
        yield return CoreGameState.StartCoroutine(CoreGameState.DisplayAllUnitStatus(2f));

        // End-of-round checks
        if (CoreGameState.players.All(player => player.IsDead()))
        {
            Debug.Log("All players dead! Game Over!");
            yield break;
        }

        if (CoreGameState.enemies.All(enemy => enemy.IsDead()))
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
        for (int i = 0; i < queue.Count; i++)
        {
            var action = queue.ElementAt(i);
            var unit = action.User;
            if (unit == null || unit.IsDead() || action.ChosenAbility == null)
                continue;

            yield return new WaitForSeconds(1f);

            // ------- Movement for acting unit -------
            if (action.ChosenAbility.MoveSelf != 0)
            {
                int newUnitIndex = Mathf.Clamp((int)unit.CurrentPosition + action.ChosenAbility.MoveSelf, 0, 2);
                Move(unit, allUnits, newUnitIndex);
                yield return new WaitForSeconds(1f);
            }
            // ------- Movement for acting unit -------

            // Process each target position of the ability
            foreach (var targetPos in action.ChosenAbility.targetPositions)
            {
                var unitsInPos = targets.Where(u => u.CurrentPosition == targetPos && !u.IsDead()).ToList();

                foreach (var target in unitsInPos)
                {
                    bool hit = Random.value <= action.ChosenAbility.hitChance;
                    if (!hit)
                    {
                        Debug.Log(unit.Stats.charName + " missed " + target.Stats.charName);
                        continue;
                    }

                    // Heal / Buff / Damage logic
                    if (action.ChosenAbility.isHeal)
                    {
                        if (unit.IsPlayer)
                        {
                            int healAmount = action.ChosenAbility.damage;

                            // Heal all alive players
                            foreach (var p in CoreGameState.players.Where(p => !p.IsDead()))
                            {
                                p.CurrentVigor += healAmount;
                                Debug.Log(unit.Stats.charName + " heals " + p.Stats.charName + " for " + healAmount);
                            }
                        }
                        else
                        {
                            // Enemy only heals self
                            target.CurrentVigor += action.ChosenAbility.damage;
                            Debug.Log(unit.Stats.charName + " heals self for " + action.ChosenAbility.damage);
                        }
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

                    // ------- Movement for target -------
                    if (action.ChosenAbility.MoveTarget != 0)
                    {
                        int targetIndex = Mathf.Clamp((int)target.CurrentPosition + action.ChosenAbility.MoveTarget, 0, 2);
                        Move(target, targets, targetIndex);
                        yield return new WaitForSeconds(1f);
                    }
                    // ------- Movement for target -------
                }
            }
        }
    }

    // Moving Around with 
    private void Move(TurnOrder.Unit unit, List<TurnOrder.Unit> allUnits, int newPos)
    {
        var occupiedUnit = allUnits.FirstOrDefault(u => u != unit && u.CurrentPosition == (TurnOrder.Positions)newPos && !u.IsDead());
        if (occupiedUnit != null)
        {
            // Swap positions
            int oldPos = (int)unit.CurrentPosition;
            unit.CurrentPosition = (TurnOrder.Positions)newPos;
            occupiedUnit.CurrentPosition = (TurnOrder.Positions)oldPos;

            if (unit.IsPlayer)
                CoreGameState.movingPositions.MovePlayer(allUnits.IndexOf(unit), newPos);
            else
                CoreGameState.movingPositions.MoveEnemy(allUnits.IndexOf(unit), newPos);

            if (occupiedUnit.IsPlayer)
                CoreGameState.movingPositions.MovePlayer(allUnits.IndexOf(occupiedUnit), oldPos);
            else
                CoreGameState.movingPositions.MoveEnemy(allUnits.IndexOf(occupiedUnit), oldPos);
        }
        else
        {
            unit.CurrentPosition = (TurnOrder.Positions)newPos;
            if (unit.IsPlayer)
                CoreGameState.movingPositions.MovePlayer(allUnits.IndexOf(unit), newPos);
            else
                CoreGameState.movingPositions.MoveEnemy(allUnits.IndexOf(unit), newPos);
        }

        Debug.Log(unit.Stats.charName + " moved to " + unit.CurrentPosition);
    }
}
