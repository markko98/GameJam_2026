using System.Collections.Generic;
using UnityEngine;

public class GameplayController : USceneController
{
    private GameplayOutlet outlet;
    private GridManager gridManager;
    private readonly LevelType levelType;

    private UIStackNavigationController navigationController;
    private MaskInteractionView maskInteractionView;

    public GameplayController(LevelType levelType) : base(SceneNames.Gameplay)
    {
        this.levelType = levelType;
    }
    
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        outlet = GameObject.Find(OutletNames.Gameplay).GetComponent<GameplayOutlet>();
        SetupGrid();
        
        SetupMaskInteractionView();
    }

    private void SetupGrid()
    {
        var levelData = LevelDataProvider.GetLevelData(levelType);
        gridManager = new GridManager(levelData, outlet.rightGridRoot, outlet.leftGridRoot);
        gridManager.SpawnAndAnimateGrid();
    }

    private void SetupMaskInteractionView()
    {
        navigationController ??= new UIStackNavigationController();
        maskInteractionView ??= new MaskInteractionView(outlet.canvas.transform, navigationController);
        
        maskInteractionView?.PresentView();
    }
}