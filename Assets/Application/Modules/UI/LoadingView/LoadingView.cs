using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

public class LoadingView : UIViewController
{
    private LoadingViewOutlet outlet;
    private readonly Action onLoadedCallback;
    private readonly float duration;
    private DisposeBag disposeBag;

    public LoadingView(Action onLoadedCallback, float duration, Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        this.onLoadedCallback = onLoadedCallback;
        this.duration = duration;
        disposeBag = new DisposeBag();
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "LoadingView");
        view = Object.Instantiate(prefab, viewport, false);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<LoadingViewOutlet>();

    }

    public override void ViewWillAppear()
    {
        base.ViewWillAppear();
        SetLoadingProgress(0f);
    }

    public void SetLoadingProgress(float progress)
    {
        if (outlet != null && outlet.loadingBarFill != null)
            outlet.loadingBarFill.fillAmount = Mathf.Clamp01(progress);
        DOVirtual.Float(0f, 1f, duration, value => outlet.loadingBarFill.fillAmount = value);
        DelayedExecutionManager.ExecuteActionAfterDelay((int)(duration * 1000), OnComleted).disposeBy(disposeBag);
    }

    private void OnComleted()
    {
        onLoadedCallback?.Invoke();
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        disposeBag?.Dispose();
    }
}
