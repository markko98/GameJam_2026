using UnityEngine;

public class TestEvent : IEvent
{
    public readonly bool isTest;

    public TestEvent(bool isTest)
    {
        this.isTest = isTest;
    }
}