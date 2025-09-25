using System.Linq;
using UnityEngine;

public class PlayerChoiceState : CoreGameplayLoop
{
    public PlayerChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        Debug.Log("===== Player Choice Phase =====");

        foreach (var player in CoreGameState.players.Where(p => !p.IsDead()))
        {
            CoreGameState.TriggerPlayerTurnStart(player); 
        }
    }

    public override void Update() { }
    public override void Exit() { }

    public void PlayerUseSkill(TurnOrder.Unit player, SkillData chosenSkill)
    {
        if (player.IsDead()) return;
        CoreGameState.playerActionsQueue.Enqueue(new TurnOrder.PlayerActionType(player, chosenSkill));
        Debug.Log(player.Stats.charName + " will use " + chosenSkill.skillName);
        CoreGameState.ChangeState(new ActionPhaseState(CoreGameState));
    }
}
