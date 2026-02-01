using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameOverView : UIViewController
{
    private GameOverViewOutlet outlet;
    private readonly Action onRestartAction;
    private readonly Action onExitAction;

    public GameOverView(Action onRestartAction, Action onExitAction, Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        this.onRestartAction = onRestartAction;
        this.onExitAction = onExitAction;

        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "GameOverView");
        view = Object.Instantiate(prefab, viewport, false);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<GameOverViewOutlet>();
    }

    public override void ViewWillAppear()
    {
        outlet.restartButton.button.onClick.AddListener(Restart);
    }

    private void Restart()
    {
        onRestartAction?.Invoke();
    }
    
    private void Exit()
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
        outlet.restartButton.button.onClick.RemoveListener(Restart);
        outlet.exitButton.button.onClick.RemoveListener(Exit);

    }
}
