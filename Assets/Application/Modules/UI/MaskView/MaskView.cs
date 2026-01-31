
public class MaskView
{
    private readonly MaskType maskType;
    private readonly MaskViewOutlet outlet;
    private EventBinding<PauseEvent> pauseBinding;
    private bool isPaused;

    public MaskView(MaskType maskType, MaskViewOutlet outlet)
    {
        this.maskType = maskType;
        this.outlet = outlet;
        
        Initialize();
    }

    private void Initialize()
    {
        pauseBinding = new EventBinding<PauseEvent>(OnPauseChanged);
        UEventBus<PauseEvent>.Register(pauseBinding);
        outlet.button.onClick.AddListener(MaskTriggerAttempt);
        outlet.nameText.text = maskType.ToString();
        outlet.image.sprite = SpriteProvider.GetMaskSprite(maskType);
    }

    private void OnPauseChanged(PauseEvent args)
    {
        isPaused = args.isPaused;
    }

    private void MaskTriggerAttempt()
    {
        if (isPaused) return;
        UEventBus<MaskTriggerAttemptEvent>.Raise(new MaskTriggerAttemptEvent() {maskType = maskType});
    }

    public void CleanUp()
    {        
        UEventBus<PauseEvent>.Deregister(pauseBinding);
        outlet.button.onClick.RemoveAllListeners();
    }
}