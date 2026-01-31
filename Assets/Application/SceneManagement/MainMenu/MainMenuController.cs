using System;
using UnityEngine;

public class MainMenuController : USceneController
{
    private MainMenuOutlet outlet;
    private UIStackNavigationController navigationController;
    private SettingsView settingsView;

    public MainMenuController() : base(SceneNames.MainMenu)
    {
    }
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        outlet = GameObject.Find(OutletNames.MainMenu).GetComponent<MainMenuOutlet>();
        outlet.newGameButton.button.onClick.AddListener(OpenGameplay);
        outlet.continueButton.button.onClick.AddListener(ContinueGameplay);
        outlet.continueButton.Toggle(false);
        outlet.settingsButton.button.onClick.AddListener(OpenSettings);
        outlet.exitButton.button.onClick.AddListener(ExitGame);
    }

    private void ContinueGameplay()
    {
    }

    private void OpenGameplay()
    {
        var gameplay = new GameplayController(LevelType.Level1);
        PushSceneController(gameplay);
    }

    private void OpenSettings()
    {
        navigationController ??= new UIStackNavigationController();

        settingsView ??= new SettingsView(outlet.canvas.transform, navigationController);

        settingsView?.PresentView(0f, AnimationType.SlideInRight);
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    public override void SceneWillDisappear()
    {
        outlet.newGameButton.button.onClick.AddListener(OpenGameplay);
        outlet.settingsButton.button.onClick.AddListener(OpenSettings);
    }
}