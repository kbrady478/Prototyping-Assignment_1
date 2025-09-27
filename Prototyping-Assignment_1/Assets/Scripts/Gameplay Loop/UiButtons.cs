using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UiButton : MonoBehaviour
{
    public TurnOrder turnOrder;
    public Button skill1Button;
    public Button skill2Button;
    public Button skill3Button;

    public TMP_Text skill1Text;
    public TMP_Text skill2Text;
    public TMP_Text skill3Text;

    private TurnOrder.Unit currentPlayer;

    private void OnEnable()
    {
        turnOrder.OnPlayerTurnStart += OnPlayerTurnStart;

        // Hide buttons at the start
        SetButtonsActive(false);
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

        SetButtonsActive(true);

        RewriteButton(player);

        Debug.Log("Player turn start: " + player.Stats.charName);

        // Assign button actions for current player
        skill1Button.onClick.RemoveAllListeners();
        skill2Button.onClick.RemoveAllListeners();
        skill3Button.onClick.RemoveAllListeners();

        if (player.Stats.skills.Length > 0)
            skill1Button.onClick.AddListener(() => UseSkill(player.Stats.skills[0]));

        if (player.Stats.skills.Length > 1)
            skill2Button.onClick.AddListener(() => UseSkill(player.Stats.skills[1]));

        if (player.Stats.skills.Length > 2)
            skill3Button.onClick.AddListener(() => UseSkill(player.Stats.skills[2]));
    }

    private void UseSkill(SkillData chosenSkill)
    {
        if (currentPlayer == null || currentPlayer.IsDead()) return;

        // Tell PlayerChoiceState that this player chose a skill
        var playerChoiceState = turnOrder.CurrentState as PlayerChoiceState;
        if (playerChoiceState != null)
        {
            playerChoiceState.PlayerUseSkill(currentPlayer, chosenSkill);
        }
        else
        {
            turnOrder.playerActionsQueue.Enqueue(new TurnOrder.PlayerActionType(currentPlayer, chosenSkill));
        }

        Debug.Log(currentPlayer.Stats.charName + " chose " + chosenSkill.skillName);

        // Disable button
        SetButtonsActive(false);

    }


    private void RewriteButton(TurnOrder.Unit player)
    {
        
        skill1Text.text = "-";
        skill2Text.text = "-";
        skill3Text.text = "-";
        
        // Checks what skills is equipped on the character
        if (player.Stats.skills.Length > 0)
            skill1Text.text = player.Stats.skills[0].skillName;

        if (player.Stats.skills.Length > 1)
            skill2Text.text = player.Stats.skills[1].skillName;

        if (player.Stats.skills.Length > 2)
            skill3Text.text = player.Stats.skills[2].skillName;
    }

    private void SetButtonsActive(bool state)
    {
        skill1Button.gameObject.SetActive(state);
        skill2Button.gameObject.SetActive(state);
        skill3Button.gameObject.SetActive(state);
    }
}
