using System.Collections.Generic;
using UnityEngine;

public class LoadingView : UIViewController
{
    private LoadingViewOutlet outlet;

    public LoadingView(Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "LoadingView");
        view = Object.Instantiate(prefab, viewport, false);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<LoadingViewOutlet>();

        SetLoadingProgress(0f);
    }

    public void SetLoadingProgress(float progress)
    {
        if (outlet != null && outlet.loadingBarFill != null)
            outlet.loadingBarFill.fillAmount = Mathf.Clamp01(progress);
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }
}
