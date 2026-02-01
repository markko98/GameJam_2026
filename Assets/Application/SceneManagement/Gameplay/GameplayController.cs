using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GameplayController : USceneController
{
    private GameplayOutlet outlet;
    private GridManager gridManager;
    private readonly LevelType levelType;

    private UIStackNavigationController navigationController;
    private MaskInteractionView maskInteractionView;
    private PauseView pauseView;
    private LoadingView loadingView;
    private LevelCompleteView levelCompletedView;

    private EventBinding<LevelCompletedEvent> levelCompletedBinding;
    private EventBinding<PlayerDiedEvent> playerDiedBinding;
    private LevelSO levelData;
    
    private GameOverView gameOverView;

    public GameplayController(LevelType levelType) : base(SceneNames.Gameplay)
    {
        this.levelType = levelType;
    }
    
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        levelCompletedBinding = new EventBinding<LevelCompletedEvent>(OnLevelCompletedCallback);
        UEventBus<LevelCompletedEvent>.Register(levelCompletedBinding);
        outlet = GameObject.Find(OutletNames.Gameplay).GetComponent<GameplayOutlet>();
        navigationController ??= new UIStackNavigationController();

        levelData = LevelDataProvider.GetLevelData(levelType);

        playerDiedBinding = new EventBinding<PlayerDiedEvent>(ShowGameOverView);
        UEventBus<PlayerDiedEvent>.Register(playerDiedBinding);

        outlet.EndGameManager.Initialize();
        
        outlet.pauseButton.button.onClick.AddListener(ShowPause);
        SetupGrid();
        SetupSkyBox();
        SetupMaskInteractionView();
    }


    private void SetupSkyBox()
    {
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
        RenderSettings.skybox = GameplayAssetProvider.GetSkybox(levelType);
    }

    private void SetupGrid()
    {
        gridManager = new GridManager(levelData, outlet.rightGridRoot, outlet.leftGridRoot);
        gridManager.SpawnAndAnimateGrid();
    }

    private void SetupMaskInteractionView()
    {
        maskInteractionView ??= new MaskInteractionView(outlet.canvas.transform, navigationController);
        
        maskInteractionView?.PresentView();
    }

    private void ShowPause()
    {
        UEventBus<PauseEvent>.Raise(new PauseEvent(true));
        pauseView ??= new PauseView(OnResumeCallback, OnExitCallback, outlet.canvas.transform, navigationController);
        pauseView?.PresentView(0f, AnimationType.SlideInDown);
    }

    private void ShowGameOverView()
    {
        gameOverView ??= new GameOverView(RestartGame, OnExitCallback, outlet.canvas.transform, navigationController);
        gameOverView?.PresentView(0f, AnimationType.SlideInUp);
    }

    private void RestartGame()
    {
        var gameplay = new GameplayController(levelType);
        PushSceneController(gameplay);
    }
    
    
    private void OnLevelCompletedCallback(LevelCompletedEvent obj)
    {
        SaveLevelIndex();
        UnlockMask();
        ShowLevelCompleted();
    }

    private void ShowLevelCompleted()
    {
        if (levelData.unlockedMask == MaskType.None)
        {
            OnExitCallback();
        }
        var maskSprite = SpriteProvider.GetMaskSprite(levelData.unlockedMask);
        var description = LevelDataProvider.GetMaskDescription(levelData.unlockedMask);
        levelCompletedView ??= new LevelCompleteView(OnContinueLevelCompleteCallback, outlet.canvas.transform, navigationController, maskSprite, description);
        levelCompletedView?.PresentView(0f, AnimationType.SlideInUp);
    }

    private void OnContinueLevelCompleteCallback()
    {
        GoToNextLevel();
    }

    private void OnExitCallback()
    {
        UEventBus<PauseEvent>.Raise(new PauseEvent(false));
        UNavigationController.PopToRootViewController();
    }

    private void OnResumeCallback()
    {
        UEventBus<PauseEvent>.Raise(new PauseEvent(false));
    }

    private void UnlockMask()
    {
        var mask = levelData.unlockedMask;
        if (mask == MaskType.None) return;

        var unlocked = ServiceProvider.storage.LoadUnlockedMasks();
        if (unlocked.Contains(mask)) return;

        unlocked.Add(mask);
        ServiceProvider.storage.SaveUnlockedMasks(unlocked);
    }

    private void SaveLevelIndex()
    {
        var levelIndex = ServiceProvider.storage.LoadInt(StorageKeys.MaxLevelIndex);

        if (levelIndex > (int)levelType) return;

        int maxLevel = Enum.GetValues(typeof(LevelType)).Length - 1;
        levelIndex = Math.Clamp((int)levelType + 1, 0, maxLevel);

        ServiceProvider.storage.SaveInt(StorageKeys.MaxLevelIndex, levelIndex);
    }

    private void GoToNextLevel()
    {
        loadingView ??= new LoadingView(OnLoadedCallback, 2f, outlet.canvas.transform, navigationController);
        loadingView?.PresentView(0f, AnimationType.ScaleUpFromMiddle);
    }

    private void OnLoadedCallback()
    {
        loadingView.RemoveView();
        int maxLevel = Enum.GetValues(typeof(LevelType)).Length - 1;
        var nextLevel = (LevelType)Math.Clamp((int)levelType + 1, 0, maxLevel);
        var gameplay = new GameplayController(nextLevel);
        PushSceneController(gameplay);
    }
    public override void SceneWillDisappear()
    {
        base.SceneWillDisappear();
        outlet.pauseButton.button.onClick.RemoveListener(ShowPause);
        UEventBus<LevelCompletedEvent>.Deregister(levelCompletedBinding);
        UEventBus<PlayerDiedEvent>.Deregister(playerDiedBinding);
        playerDiedBinding = null;

        levelCompletedBinding = null;
        levelCompletedView?.RemoveView();
        pauseView?.RemoveView();
        pauseView = null;
        gridManager?.CleanUp();
        maskInteractionView?.RemoveView();
        maskInteractionView?.Cleanup();
        outlet.EndGameManager.Cleanup();
        levelCompletedView = null;
        gameOverView?.Cleanup();
        gameOverView?.RemoveView();
        gameOverView = null;
    }
}