using UnityEngine;

public class MaskInteractionManager
{
    private readonly EventBinding<MaskTriggerAttemptEvent> maskTriggeredAttemptEvent;
    
    private MaskType activeMaskType = MaskType.None;
    
    private float currentMaskTimer;
    private const float maxMaskTimer = 5f;

    private float maskCooldownTimer;
    private const float maxCooldownTimer = 2f;

    private bool isMaskCooldownActive => maskCooldownTimer > 0;

    public MaskInteractionManager()
    {
        maskTriggeredAttemptEvent = new EventBinding<MaskTriggerAttemptEvent>(OnMaskTriggerAttempt);
        UEventBus<MaskTriggerAttemptEvent>.Register(maskTriggeredAttemptEvent);
        
        GameTicker.SharedInstance.Update += Update;
    }

    private void Update()
    {
        if (isMaskCooldownActive)
            maskCooldownTimer -= GameTicker.DeltaTime;

        if (activeMaskType == MaskType.None) return;

        currentMaskTimer += GameTicker.DeltaTime;

        if (currentMaskTimer >= maxMaskTimer)
        {
            OnMaskExpired();
        }
    }

    private void OnMaskTriggerAttempt(MaskTriggerAttemptEvent e)
    {
        if (isMaskCooldownActive) return; // maybe play some sound or ui
        
        activeMaskType = e.maskType;
        currentMaskTimer = 0f;
        maskCooldownTimer = maxCooldownTimer;
        UEventBus<MaskTriggeredEvent>.Raise(new MaskTriggeredEvent {maskType = activeMaskType});
    }
    
    private void OnMaskExpired()
    {
        if (activeMaskType == MaskType.None) return;
        
        UEventBus<MaskExpiredEvent>.Raise(new MaskExpiredEvent {maskType = activeMaskType});
        
        activeMaskType = MaskType.None;
        currentMaskTimer = 0f;
    }

    public void CleanUp()
    {
        UEventBus<MaskTriggerAttemptEvent>.Deregister(maskTriggeredAttemptEvent);
        GameTicker.SharedInstance.Update -= Update;
    }
}