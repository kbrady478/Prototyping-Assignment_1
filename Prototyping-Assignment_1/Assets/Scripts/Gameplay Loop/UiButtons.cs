using UnityEngine;
using UnityEngine.UI;

public class UiButton : MonoBehaviour
{
    public TurnOrder turnOrder;
    public Button skill1Button;
    public Button skill2Button;
    public Button skill3Button;

    private TurnOrder.Unit currentPlayer;

    private void OnEnable()
    {
        turnOrder.OnPlayerTurnStart += OnPlayerTurnStart;
    }

    private void OnDisable()
    {
        turnOrder.OnPlayerTurnStart -= OnPlayerTurnStart;
    }

    private void OnPlayerTurnStart(TurnOrder.Unit player)
    {
        currentPlayer = player;

        if (player.IsDead())
        {
            // Skip dead player and go to next alive player
            turnOrder.NextAlivePlayerTurn();
            return;
        }

        Debug.Log("Player turn start: " + player.Stats.charName);

        // Assign button actions for current player
        skill1Button.onClick.RemoveAllListeners();
        skill1Button.onClick.AddListener(() => UseSkill(currentPlayer.Stats.skills[0]));

        skill2Button.onClick.RemoveAllListeners();
        skill2Button.onClick.AddListener(() => UseSkill(currentPlayer.Stats.skills[1]));

        skill3Button.onClick.RemoveAllListeners();
        skill3Button.onClick.AddListener(() => UseSkill(currentPlayer.Stats.skills[2]));
    }

    private void UseSkill(SkillData chosenSkill)
    {
        if (currentPlayer == null || currentPlayer.IsDead()) return;

        // Enqueue chosen skill
        turnOrder.playerActionsQueue.Enqueue(new TurnOrder.PlayerActionType(currentPlayer, chosenSkill));
        Debug.Log(currentPlayer.Stats.charName + " chose " + chosenSkill.skillName);

        // Move to next alive player's turn
        turnOrder.NextAlivePlayerTurn();
    }
}
