using UnityEngine;

public class GameplayController : USceneController
{
    private GameplayOutlet outlet;
    private GridManager gridManager;
    private readonly LevelType levelType;

    public GameplayController(LevelType levelType) : base(SceneNames.Gameplay)
    {
        this.levelType = levelType;
    }
    
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        outlet = GameObject.Find(OutletNames.Gameplay).GetComponent<GameplayOutlet>();
        SetupGrid();
    }

    private void SetupGrid()
    {
        var levelData = LevelDataProvider.GetLevelData(levelType);
        gridManager = new GridManager(levelData, outlet);
        gridManager.SpawnAndAnimateGrid();
    }
}