using UnityEngine;

public class PlayerChoiceState : CoreGameplayLoop
{
    public PlayerChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter() => Debug.Log("===== Player Choice Phase =====");

    public override void Update() { }
    public override void Exit() { }

    public void PlayerUseSkill(TurnOrder.Unit player, SkillData chosenSkill)
    {
        if (player.IsDead()) return;
        TurnOrder.Positions targetPos = chosenSkill.targetPositions[0]; // choose first position as default
        CoreGameState.playerActionsQueue.Enqueue(new TurnOrder.PlayerActionType(chosenSkill, targetPos, player));

        Debug.Log(player.Stats.charName + " will use " + chosenSkill.skillName + " and move to " + chosenSkill.MoveSelf);
    }
}
