using UnityEngine;

public class TestEvent : IEvent
{
    public readonly bool isTest;

    public TestEvent(bool isTest)
    {
        this.isTest = isTest;
    }
}

public class MaskTriggerAttemptEvent : IEvent
{
    public MaskType maskType;
}

public class MaskTriggeredEvent : IEvent
{
    public MaskType maskType;
}

public class MaskExpiredEvent : IEvent
{
    public MaskType maskType;
}

public class PauseEvent : IEvent
{
    public bool isPaused;

    public PauseEvent(bool isPaused)
    {
        this.isPaused = isPaused;
    }
}

public class PlayerGoalDetectionEvent : IEvent
{
    public bool isInGoal;
    public PlayerSide playerSide;
    public GameObject player;

    public PlayerGoalDetectionEvent(bool isInGoal, PlayerSide playerSide, GameObject player)
    {
        this.isInGoal = isInGoal;
        this.playerSide = playerSide;
        this.player = player;
    }
}


public class LevelCompletedEvent : IEvent
{
    public LevelCompletedEvent()
    {
    }
}