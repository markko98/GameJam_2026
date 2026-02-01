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

    public GameplayController(LevelType levelType) : base(SceneNames.Gameplay)
    {
        this.levelType = levelType;
    }
    
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        outlet = GameObject.Find(OutletNames.Gameplay).GetComponent<GameplayOutlet>();
        navigationController ??= new UIStackNavigationController();
        navigationPauseController ??= new UIStackNavigationController();
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
        var levelData = LevelDataProvider.GetLevelData(levelType);
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
        
        gridManager.CleanUp();
        maskInteractionView.Cleanup();
    }
}