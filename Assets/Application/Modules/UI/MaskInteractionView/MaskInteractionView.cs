using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

public class MaskInteractionView: UIViewController
{
    private MaskInteractionViewOutlet outlet;
    private readonly List<MaskView> maskViews = new List<MaskView>();
    private DisposeBag disposeBag = new ();
    
    private readonly MaskInteractionManager manager;
    
    private EventBinding<MaskTriggeredEvent> maskTriggeredEvent;
    private EventBinding<MaskExpiredEvent> maskExpiredEvent;

    public MaskInteractionView(Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "MaskInteractionView");
        view = Object.Instantiate(prefab, viewport, false);
        manager = new MaskInteractionManager(outlet.inputAsset);
        
        GameTicker.SharedInstance.Update += Update;

        maskTriggeredEvent = new EventBinding<MaskTriggeredEvent>(OnMaskTriggered);
        UEventBus<MaskTriggeredEvent>.Register(maskTriggeredEvent);
        
        maskExpiredEvent = new EventBinding<MaskExpiredEvent>(OnMaskExpired);
        UEventBus<MaskExpiredEvent>.Register(maskExpiredEvent);
    }

    private void OnMaskTriggered(MaskTriggeredEvent obj)
    {
        outlet.loadingBarFill.color = BlockView.GetColorForMaskType(obj.maskType);
        outlet.loadingBarFill.fillAmount = 1f;
        outlet.loadingBar.transform.DOLocalMoveY(-80f, 0.3f);
        
        outlet.cooldownImage.fillAmount = 1f;
        outlet.cooldownImage.transform.DOMoveY(400f, 0.3f);
    }

    private void Update()
    {
        if (manager.isMaskActive)
        {
            SetLoadingProgress(manager.maskProgress);
        }

        if (manager.isMaskCooldownActive)
        {
            outlet.cooldownImage.DOFillAmount(Mathf.Clamp01(manager.cooldownProgress), 0.2f).SetEase(Ease.Linear);
        }
        else
        {
            if (Mathf.Approximately(outlet.cooldownImage.transform.position.y, 50f)) return;
            
            outlet.cooldownImage.transform.DOMoveY(50f,0.3f);
        }
    }
    
    private void SetLoadingProgress(float progress)
    {
        outlet.loadingBarFill.DOFillAmount(Mathf.Clamp01(progress), .2f).SetEase(Ease.Linear);
    }

    private void OnMaskExpired()
    {
        outlet.loadingBar.transform.DOLocalMoveY(138f, 0.3f);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponent<MaskInteractionViewOutlet>();
        
        SetupMasks();
    }

    private void SetupMasks()
    {
        var masks = ServiceProvider.storage.LoadUnlockedMasks();

        if (masks.Count == 0) return;
        
        foreach (var maskData in outlet.masksData)
        {
            bool isUnlocked = masks.Contains(maskData.MaskType);
            var maskView = new MaskView(maskData.MaskType, !isUnlocked, maskData.MaskViewOutlet);
            maskViews.Add(maskView);
        }
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        
        manager.CleanUp();
        foreach (var maskView in maskViews)
        {
           maskView.CleanUp(); 
        }
        
        maskViews.Clear();
        GameTicker.SharedInstance.Update -= Update;
        UEventBus<MaskTriggeredEvent>.Deregister(maskTriggeredEvent);
        UEventBus<MaskExpiredEvent>.Deregister(maskExpiredEvent);
        maskTriggeredEvent = null;
        maskExpiredEvent = null;
    }
}