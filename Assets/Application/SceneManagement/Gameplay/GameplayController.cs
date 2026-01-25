using UnityEngine;

public class GameplayController : USceneController
{
    private GameplayOutlet outlet;

    public GameplayController() : base(SceneNames.Gameplay)
    {
    }
    
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        outlet = GameObject.Find(OutletNames.Gameplay).GetComponent<GameplayOutlet>();
    }
}