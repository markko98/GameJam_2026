using System.Collections.Generic;
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
        PopToParentSceneController();
    }

    private void OnResumeCallback()
    {
        UEventBus<PauseEvent>.Raise(new PauseEvent(false));
    }

    public override void SceneWillDisappear()
    {
        base.SceneWillDisappear();
        
        gridManager.CleanUp();
        maskInteractionView.Cleanup();
    }
}