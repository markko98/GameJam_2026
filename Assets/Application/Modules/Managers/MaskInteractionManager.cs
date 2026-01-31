using UnityEngine;

public class MaskInteractionManager
{
    private readonly EventBinding<MaskTriggeredEvent> maskTriggeredEvent;
    
    private MaskType activeMaskType = MaskType.None;
    
    private float currentMaskTimer;
    private const float maxMaskTimer = 10f;

    private float maskCooldownTimer;
    private const float maxCooldownTimer = 1f;

    private bool isMaskCooldownActive => maskCooldownTimer > 0;

    public MaskInteractionManager()
    {
        maskTriggeredEvent = new EventBinding<MaskTriggeredEvent>(OnMaskTriggered);
        UEventBus<MaskTriggeredEvent>.Register(maskTriggeredEvent);
        
        GameTicker.SharedInstance.Update += Update;
    }

    private void Update()
    {
        if (activeMaskType == MaskType.None) return;
        
        currentMaskTimer += GameTicker.DeltaTime;

        if (currentMaskTimer >= maxMaskTimer)
        {
            OnMaskExpired();
        }

        if (maskCooldownTimer <= 0 ) return;
        maskCooldownTimer -= GameTicker.DeltaTime;
    }

    private void OnMaskTriggered(MaskTriggeredEvent e)
    {
        if (isMaskCooldownActive) return; // maybe play some sound or ui

        activeMaskType = e.maskType;
        maskCooldownTimer = maxCooldownTimer;
        
        Debug.Log(activeMaskType);
        
    }

    private void OnMaskExpired()
    {
        if (activeMaskType == MaskType.None) return;
        
        Debug.Log("Mask expired" + activeMaskType);

        UEventBus<MaskExpiredEvent>.Raise(new MaskExpiredEvent {maskType = activeMaskType});
        
        activeMaskType = MaskType.None;
        currentMaskTimer = 0f;
    }

    private void CleanUp()
    {
        UEventBus<MaskTriggeredEvent>.Deregister(maskTriggeredEvent);
        GameTicker.SharedInstance.Update -= Update;
    }
}