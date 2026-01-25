using System;

public class CountdownTimer
{
    private float timer;

    private float countdownTime;
    private Action onGoalReached;
    private bool canUpdate;
    
    private bool timerTriggered = false;
    private Action<float> onTimerChanged;

    public CountdownTimer(float countdownTime, Action onGoalReached, Action<float> onTimerChanged)
    {
        this.countdownTime = countdownTime;
        this.onGoalReached = onGoalReached;
        this.onTimerChanged = onTimerChanged;
        this.timer = countdownTime;
        canUpdate = true;
        GameTicker.SharedInstance.Update += Update;
    }

    private void Update()
    {
        if(canUpdate == false) return;
        if(timerTriggered) return;
        
        timer -= GameTicker.DeltaTime;
        if (timer <= 0)
        {
            timer = 0;
            timerTriggered = true;
            onGoalReached?.Invoke();
        }
        onTimerChanged?.Invoke(timer);
    }

    public void Pause()
    {
        canUpdate = false;
    }
    
    public void Resume()
    {
        canUpdate = true;
    }

    public void Cleanup()
    {
        GameTicker.SharedInstance.Update -= Update;
        canUpdate = false;
        timer = countdownTime;
        onGoalReached = null;
        onTimerChanged = null;
    }
    
    ~CountdownTimer()
    {
        Cleanup();
    }
}