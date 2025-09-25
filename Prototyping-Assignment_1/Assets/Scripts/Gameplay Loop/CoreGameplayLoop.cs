using UnityEngine;


// This acts as the base state for the gameplay loop
public abstract class CoreGameplayLoop
{
    protected TurnOrder CoreGameState;

    public CoreGameplayLoop(TurnOrder coreGameState)
    {
        CoreGameState = coreGameState;
    }

    // Called when entering the state
    public abstract void Enter();

    // Called every frame
    public abstract void Update();

    // Called when exiting the state
    public abstract void Exit();
}
