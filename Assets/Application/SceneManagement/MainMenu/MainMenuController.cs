using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : USceneController
{
    private MainMenuOutlet outlet;
    private UIStackNavigationController navigationController;
    private SettingsView settingsView;
    private LoadingView loadingView;
    private VideoView videoView;
    private PooledAudioSource mainMenuSound;

    public MainMenuController() : base(SceneNames.MainMenu)
    {
    }
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        navigationController = new UIStackNavigationController();
        outlet = GameObject.Find(OutletNames.MainMenu).GetComponent<MainMenuOutlet>();
        outlet.newGameButton.button.onClick.AddListener(GoToNextLevel);
        outlet.continueButton.button.onClick.AddListener(ContinueGameplay);
        var levelIndex = ServiceProvider.storage.LoadInt(StorageKeys.MaxLevelIndex);
        outlet.continueButton.Toggle(levelIndex > 0);
        outlet.settingsButton.button.onClick.AddListener(OpenSettings);
        outlet.exitButton.button.onClick.AddListener(ExitGame);

        mainMenuSound = ServiceProvider.audioService.PlayAmbience(SoundIds.music_main_menu);

        UpdateTimeBasedBackground();
    }

    private void ShowLoadingScreen()
    {
        loadingView ??= new LoadingView(OnLoadedCallback, 2f, outlet.canvas.transform, navigationController);
        loadingView?.PresentView(0f, AnimationType.ScaleUpFromMiddle);
    }

    private void ContinueGameplay()
    {
        ServiceProvider.audioService.StopAmbience(mainMenuSound);
        ShowLoadingScreen();
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
        ServiceProvider.storage.SaveInt(StorageKeys.MaxLevelIndex, 0);
        ServiceProvider.storage.SaveUnlockedMasks(new List<MaskType>() { MaskType.Kane });
        
        ServiceProvider.audioService.StopAmbience(mainMenuSound);
        videoView ??= new VideoView(ShowLoadingScreen, outlet.canvas.transform, navigationController);
        videoView.PresentView(0f, AnimationType.ScaleUpFromMiddle);
    }

    private void OnLoadedCallback()
    {
        var levelIndex = ServiceProvider.storage.LoadInt(StorageKeys.MaxLevelIndex);
        
        var gameplay = new GameplayController((LevelType)levelIndex);
        PushSceneController(gameplay);
    }

    private void OpenSettings()
    {
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
        videoView = null;
        settingsView?.RemoveView();
        settingsView = null;
        outlet.newGameButton.button.onClick.AddListener(GoToNextLevel);
        outlet.settingsButton.button.onClick.AddListener(OpenSettings);
    }
}