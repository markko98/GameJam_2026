using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PauseView : UIViewController
{
    private PauseViewOutlet outlet;
    private readonly Action onResumeAction;
    private readonly Action onExitAction;

    public PauseView(Action onResumeAction, Action onExitAction, Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        this.onResumeAction = onResumeAction;
        this.onExitAction = onExitAction;
        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "PauseView");
        view = Object.Instantiate(prefab, viewport, false);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<PauseViewOutlet>();
    }

    public override void ViewWillAppear()
    {
        outlet.mainMenuButton.button.onClick.AddListener(OpenMainMenu);
        outlet.closeButton.button.onClick.AddListener(ExitPause);
        outlet.continueButton.button.onClick.AddListener(ExitPause);
    }

    private void ExitPause()
    {
        onResumeAction?.Invoke();
        RemoveView(0f, null, AnimationType.SlideOutDown);
    }

    private void OpenMainMenu()
    {
        onExitAction?.Invoke();
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        outlet.mainMenuButton.button.onClick.RemoveListener(OpenMainMenu);
        outlet.closeButton.button.onClick.RemoveListener(ExitPause);
        outlet.continueButton.button.onClick.RemoveListener(ExitPause);
    }
}
