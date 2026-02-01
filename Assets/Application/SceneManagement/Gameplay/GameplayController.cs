using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GameplayController : USceneController
{
    private GameplayOutlet outlet;
    private GridManager gridManager;
    private readonly LevelType levelType;

    private UIStackNavigationController navigationController;
    private UIStackNavigationController navigationPauseController;
    private MaskInteractionView maskInteractionView;
    private PauseView pauseView;
    private LoadingView loadingView;
    private LevelCompleteView levelCompletedView;

    private EventBinding<LevelCompletedEvent> levelCompletedBinding;
    private LevelSO levelData;

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
        navigationPauseController ??= new UIStackNavigationController();
        levelData = LevelDataProvider.GetLevelData(levelType);

        outlet.EndGameManager.Initialize();
        
        var masks = new List<MaskType>()
        {
            MaskType.Kane, MaskType.Lono, MaskType.Ku, MaskType.Kanaloa
        };
        ServiceProvider.storage.SaveUnlockedMasks(masks);
        
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
        pauseView ??= new PauseView(OnResumeCallback, OnExitCallback, outlet.canvas.transform, navigationPauseController);
        pauseView?.PresentView(0f, AnimationType.SlideInDown);
    }
    
    
    private void OnLevelCompletedCallback(LevelCompletedEvent obj)
    {
        ShowLevelCompleted();
    }

    private void ShowLevelCompleted()
    {
        var maskSprite = SpriteProvider.GetMaskSprite(levelData.unlockedMask);
        var description = LevelDataProvider.GetMaskDescription(levelData.unlockedMask);
        levelCompletedView ??= new LevelCompleteView(OnContinueLevelCompleteCallback, outlet.canvas.transform, navigationPauseController, maskSprite, description);
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

    private void GoToNextLevel()
    {
        loadingView ??= new LoadingView(OnLoadedCallback, 2f, outlet.canvas.transform, navigationPauseController);
        loadingView?.PresentView(0f, AnimationType.ScaleUpFromMiddle);
    }

    private void OnLoadedCallback()
    {
        loadingView.RemoveView();
        // TODO - load from storage
        var gameplay = new GameplayController(LevelType.Level2);
        PushSceneController(gameplay);
    }
    public override void SceneWillDisappear()
    {
        base.SceneWillDisappear();
        outlet.pauseButton.button.onClick.RemoveListener(ShowPause);
        UEventBus<LevelCompletedEvent>.Deregister(levelCompletedBinding);
        levelCompletedBinding = null;
        levelCompletedView?.RemoveView();
        pauseView?.RemoveView();
        pauseView = null;
        gridManager?.CleanUp();
        maskInteractionView?.RemoveView();
        maskInteractionView?.Cleanup();
        outlet.EndGameManager.Cleanup();
        levelCompletedView = null;
    }
}