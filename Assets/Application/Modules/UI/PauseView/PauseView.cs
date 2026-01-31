using System.Collections.Generic;
using UnityEngine;

public class PauseView : UIViewController
{
    private PauseViewOutlet outlet;

    public PauseView(Transform viewport, UIStackNavigationController controller) : base(controller)
    {
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
        outlet.exitButton.button.onClick.AddListener(ExitGame);
    }

    private void ExitPause()
    {
        RemoveView(0f, null, AnimationType.SlideOutRight);
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void OpenMainMenu()
    {
        var mainMenu = new MainMenuController();
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
        outlet.exitButton.button.onClick.RemoveListener(ExitGame);
    }
}
