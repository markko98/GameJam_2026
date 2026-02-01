
public class MaskView
{
    private readonly MaskType maskType;
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


    private void MaskTriggerAttempt()
    {
        if (isPaused) return;
        if (isLocked) return;
    }

    public void CleanUp()
    {        
        // outlet.button.onClick.RemoveAllListeners();
    }
}