using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    private EventBinding<PlayerGoalDetectionEvent> playerGoalBinding;

    private bool isRightInPlace;
    private bool isLeftInPlace;
    
    private DisposeBag disposeBag;
    private GameObject rightPlayer;
    private GameObject leftPlayer;

    public void Initialize()
    {
        // listen to events
        // if both players are in goal - trigger end 
        // disable movement
        // move players and mask and trigger gameCompletedEvent
        disposeBag = new DisposeBag();
        playerGoalBinding = new EventBinding<PlayerGoalDetectionEvent>(OnPlayerGoalInteraction);
        UEventBus<PlayerGoalDetectionEvent>.Register(playerGoalBinding);
    }

    public void Cleanup()
    {
        disposeBag?.Dispose();
        UEventBus<PlayerGoalDetectionEvent>.Deregister(playerGoalBinding);
        playerGoalBinding = null;
    }

    private void OnPlayerGoalInteraction(PlayerGoalDetectionEvent args)
    {
        if (args.playerSide is PlayerSide.Left)
        {
            isLeftInPlace = args.isInGoal;
            leftPlayer = args.player;
        }
        else if (args.playerSide is PlayerSide.Right)
        {
            isRightInPlace = args.isInGoal;
            rightPlayer = args.player;
        }

        if (isLeftInPlace && isRightInPlace)
        {
            UEventBus<PauseEvent>.Raise(new PauseEvent(true)); // check if okay
            HappyPlayers(args);
        }
    }

    private void HappyPlayers(PlayerGoalDetectionEvent args)
    {
        leftPlayer.GetComponent<PlayerController>()?.TriggerHappyAnimation();
        rightPlayer.GetComponent<PlayerController>()?.TriggerHappyAnimation();
        
        
        DelayedExecutionManager.ExecuteActionAfterDelay(2000, TriggerLevelCompleted).disposeBy(disposeBag);
    }

    private void TriggerLevelCompleted()
    {
        UEventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
    }
}