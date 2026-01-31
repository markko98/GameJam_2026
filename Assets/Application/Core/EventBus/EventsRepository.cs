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