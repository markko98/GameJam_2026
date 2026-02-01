using System;
using UnityEngine;

public class MainMenuController : USceneController
{
    private MainMenuOutlet outlet;
    private UIStackNavigationController navigationController;
    private SettingsView settingsView;
    private LoadingView loadingView;
    private UIStackNavigationController navigationLoadingController;

    public MainMenuController() : base(SceneNames.MainMenu)
    {
    }
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        navigationLoadingController = new UIStackNavigationController();
        outlet = GameObject.Find(OutletNames.MainMenu).GetComponent<MainMenuOutlet>();
        outlet.newGameButton.button.onClick.AddListener(GoToNextLevel);
        outlet.continueButton.button.onClick.AddListener(ContinueGameplay);
        outlet.continueButton.Toggle(false);
        outlet.settingsButton.button.onClick.AddListener(OpenSettings);
        outlet.exitButton.button.onClick.AddListener(ExitGame);

        UpdateTimeBasedBackground();
    }

    private void ContinueGameplay()
    {
    }

    private void UpdateTimeBasedBackground()
    {
        int hour = DateTime.Now.Hour;

        if (hour >= 6 && hour < 12)
            outlet.backgroundImage.sprite = outlet.morningBackgroundSprite;
        else if (hour >= 12 && hour < 18)
            outlet.backgroundImage.sprite = outlet.dayBackgroundSprite;
        else
            outlet.backgroundImage.sprite = outlet.nightBackgroundSprite;
    }
    
    
    private void GoToNextLevel()
    {
        loadingView ??= new LoadingView(OnLoadedCallback, 2f, outlet.canvas.transform, navigationLoadingController);
        loadingView?.PresentView(0f, AnimationType.ScaleUpFromMiddle);
    }

    private void OnLoadedCallback()
    {
        // TODO - load from storage
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
        loadingView = null;
        outlet.newGameButton.button.onClick.AddListener(GoToNextLevel);
        outlet.settingsButton.button.onClick.AddListener(OpenSettings);
    }
}