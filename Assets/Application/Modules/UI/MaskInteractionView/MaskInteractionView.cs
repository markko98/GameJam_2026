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
        manager = new MaskInteractionManager();
    }
    
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponent<MaskInteractionViewOutlet>();

        
        SetupMasks();
    }

    private void SetupMasks()
    {
        var maskViewPrefab = Resources.Load<GameObject>(Strings.UIPrefabsResourcesPath + "MaskView");
        var masks = ServiceProvider.storage.LoadUnlockedMasks();

        if (masks.Count == 0) return;
        
        foreach (var maskType in masks)
        {
            var maskGo = Object.Instantiate(maskViewPrefab, outlet.maskViewContainer, false);
            var maskViewOutlet = maskGo.GetComponent<MaskViewOutlet>();
            
            var maskView = new MaskView(maskType, maskViewOutlet);
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