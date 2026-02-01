using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class MaskInteractionView: UIViewController
{
    private MaskInteractionViewOutlet outlet;
    private readonly List<MaskView> maskViews = new List<MaskView>();
    
    private readonly MaskInteractionManager manager;
    public MaskInteractionView(Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "MaskInteractionView");
        view = Object.Instantiate(prefab, viewport, false);
        manager = new MaskInteractionManager(outlet.inputAsset);
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
    }
}