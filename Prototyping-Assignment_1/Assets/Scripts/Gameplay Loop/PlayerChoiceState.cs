using UnityEngine;

// ====== Player Choice State ======
// Handles player input (Attack, Skill, Parry, etc.)
// Resolves actions in a waiting queue list
// ====== Player Choice State ======
public class PlayerChoiceState : CoreGameplayLoop
{
    private int currentPlayerIndex = 0;

    public PlayerChoiceState(TurnOrder coreGameState) : base(coreGameState) { }

    public override void Enter()
    {
        Debug.Log("===== Player Choice Phase =====" );
        currentPlayerIndex = 0;

        NextAlivePlayer();
    }

    public override void Update() { }
    public override void Exit() { }

    // Advances index until it finds an alive player or ends turn
    private void NextAlivePlayer()
    {
        while (currentPlayerIndex < CoreGameState.players.Count &&
               CoreGameState.players[currentPlayerIndex].IsDead())
        {
            Debug.Log(CoreGameState.players[currentPlayerIndex].Stats.charName + " is dead and skips their turn.");
            currentPlayerIndex++;
        }

        if (currentPlayerIndex >= CoreGameState.players.Count)
        {
            CoreGameState.ChangeState(new ActionPhaseState(CoreGameState));
        }
        else
        {
            Debug.Log("Waiting for input from: " + CoreGameState.players[currentPlayerIndex].Stats.charName );
        }
    }

    // Player selects target and queues action
    public void PlayerTurn(int targetPosition)
    {
        if (currentPlayerIndex >= CoreGameState.players.Count) return;

        var currentPlayer = CoreGameState.players[currentPlayerIndex];

        if (currentPlayer.IsDead())
        {
            currentPlayerIndex++;
            NextAlivePlayer();
            return;
        }

        var target = (TurnOrder.Positions)targetPosition;

        CoreGameState.playerActionsQueue.Enqueue(new TurnOrder.PlayerActionType(TurnOrder.PlayerAction.Attack, target));

        Debug.Log(currentPlayer.Stats.charName + " chooses to attack " + target);

        currentPlayerIndex++;
        NextAlivePlayer();
    }
}

