using System.Linq;
using UnityEngine;
using System.Collections;

public class PlayerChoiceState : CoreGameplayLoop
{
    public PlayerChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        CoreGameState.StartCoroutine(PlayerChoiceRoutine());
    }

    public override void Update() { }
    public override void Exit() { }

    private IEnumerator PlayerChoiceRoutine()
    {
        Debug.Log("===== Player Choice Phase =====");

        // Trigger only alive players in initiative order
        foreach (var player in CoreGameState.initiativeOrderList.Where(u => u.IsPlayer && !u.IsDead()))
        {
            yield return CoreGameState.StartCoroutine(CoreGameState.NextAlivePlayerTurn());

            // Wait until the player chooses an action
            while (CoreGameState.playerActionsQueue.Count == 0)
                yield return null;

            yield return new WaitForSeconds(0.5f);
        }

        // After all players queued actions, move to action phase
        CoreGameState.ChangeState(new ActionPhaseState(CoreGameState));
    }

    public void PlayerUseSkill(TurnOrder.Unit player, SkillData chosenSkill)
    {
        if (player.IsDead()) return;

        CoreGameState.playerActionsQueue.Enqueue(new TurnOrder.PlayerActionType(player, chosenSkill));
        Debug.Log(player.Stats.charName + " will use " + chosenSkill.skillName);
    }
}
