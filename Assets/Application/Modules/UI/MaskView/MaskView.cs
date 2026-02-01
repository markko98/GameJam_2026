
using DG.Tweening;
using UnityEngine;

public class MaskView
{
    private readonly MaskType maskType;
    public MaskType MaskType => maskType;
    private readonly MaskViewOutlet outlet;
    private bool isPaused;
    private readonly bool isLocked;

    public MaskView(MaskType maskType, bool isLocked, MaskViewOutlet outlet)
    {
        this.maskType = maskType;
        this.isLocked = isLocked;
        this.outlet = outlet;
        
        Initialize();
    }

    private void Initialize()
    {
        outlet.maskImage.sprite = isLocked ? SpriteProvider.GetLockSprite() : SpriteProvider.GetMaskSprite(maskType);
    }


    public void MaskTriggerAttempt()
    {
        outlet.root.DOPunchScale(Vector3.one * 0.2f, 0.35f, 1, 1);
    }

    public void CleanUp()
    {        
    }
}