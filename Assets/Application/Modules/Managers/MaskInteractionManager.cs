using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaskInteractionManager
{
    private EventBinding<PauseEvent> pauseBinding;
    
    private MaskType activeMaskType = MaskType.None;
    
    public float currentMaskTimer;
    private const float maxMaskTimer = 5f;

    public float maskCooldownTimer;
    
    private readonly InputAction mask1Action;
    private readonly InputAction mask2Action;
    private readonly InputAction mask3Action;
    private readonly InputAction mask4Action;
    private bool isPaused;
    private readonly List<MaskType> unlockedMasks;

    private const float maxCooldownTimer = 2f;

    public bool isMaskCooldownActive => maskCooldownTimer > 0;
    public float cooldownProgress => maskCooldownTimer / maxCooldownTimer;
    
    public bool isMaskActive => activeMaskType != MaskType.None && currentMaskTimer > 0f;
    public float maskProgress => 1 - currentMaskTimer / maxMaskTimer;
    
    public MaskInteractionManager(InputActionAsset playerInput)
    {
        pauseBinding = new EventBinding<PauseEvent>(OnPauseChanged);
        UEventBus<PauseEvent>.Register(pauseBinding);
        unlockedMasks = ServiceProvider.storage.LoadUnlockedMasks();

        mask1Action = playerInput.FindAction("Mask1");
        mask2Action = playerInput.FindAction("Mask2");
        mask3Action = playerInput.FindAction("Mask3");
        mask4Action = playerInput.FindAction("Mask4");

        GameTicker.SharedInstance.Update += Update;
    }
    private void OnPauseChanged(PauseEvent args)
    {
        isPaused = args.isPaused;
    }

    private void Update()
    {
        if (isPaused) return;
        CheckInput();

        if (isMaskCooldownActive)
            maskCooldownTimer -= GameTicker.DeltaTime;

        if (activeMaskType == MaskType.None) return;

        currentMaskTimer += GameTicker.DeltaTime;

        if (currentMaskTimer >= maxMaskTimer)
        {
            OnMaskExpired();
        }

        
    }

    private void CheckInput()
    {
        var maskType = MaskType.None;
        if (mask1Action.WasPressedThisFrame())
        {
            maskType = MaskType.Kane;
        }else if (mask2Action.WasPressedThisFrame())
        {
            maskType = MaskType.Lono;
        }else if (mask3Action.WasPressedThisFrame())
        {
            maskType = MaskType.Ku;
        }else if (mask4Action.WasPressedThisFrame())
        {
            maskType = MaskType.Kanaloa;
        }

        OnMaskTriggerAttempt(maskType);
    }

    private void OnMaskTriggerAttempt(MaskType maskType)
    {
        if (isMaskCooldownActive) return; // maybe play some sound or ui
        if(unlockedMasks.Contains(maskType) == false) return;

        if (maskType != activeMaskType)
        {
            OnMaskExpired();
        }
        
        activeMaskType = maskType;
        currentMaskTimer = 0f;
        maskCooldownTimer = maxCooldownTimer;
        
        ServiceProvider.audioService.PlayOneShot(SoundIds.sfx_mask_change);
        UEventBus<MaskTriggeredEvent>.Raise(new MaskTriggeredEvent {maskType = activeMaskType});
    }
    
    private void OnMaskExpired()
    {
        if (activeMaskType == MaskType.None) return;
        
        UEventBus<MaskExpiredEvent>.Raise(new MaskExpiredEvent {maskType = activeMaskType});
        ServiceProvider.audioService.PlayOneShot(SoundIds.sfx_mask_change_expired);

        activeMaskType = MaskType.None;
        currentMaskTimer = 0f;
    }

    public void CleanUp()
    {
        UEventBus<PauseEvent>.Deregister(pauseBinding);
        GameTicker.SharedInstance.Update -= Update;
    }
}