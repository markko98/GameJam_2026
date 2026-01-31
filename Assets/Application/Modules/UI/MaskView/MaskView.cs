
public class MaskView
{
    private readonly MaskType maskType;
    private readonly MaskViewOutlet outlet;
    
    public MaskView(MaskType maskType, MaskViewOutlet outlet)
    {
        this.maskType = maskType;
        this.outlet = outlet;
        
        Initialize();
    }

    private void Initialize()
    {
        outlet.button.onClick.AddListener(() =>
        {
            UEventBus<MaskTriggerAttemptEvent>.Raise(new MaskTriggerAttemptEvent() {maskType = maskType});
        });
        
        outlet.nameText.text = maskType.ToString();
        outlet.image.sprite = SpriteProvider.GetMaskSprite(maskType);
    }

    public void CleanUp()
    {
        outlet.button.onClick.RemoveAllListeners();
    }
}