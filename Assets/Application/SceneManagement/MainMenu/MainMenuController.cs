using UnityEngine;

public class MainMenuController : USceneController
{
    private MainMenuOutlet outlet;

    public MainMenuController() : base(SceneNames.MainMenu)
    {
    }
    public override void SceneDidLoad()
    {
        base.SceneDidLoad();
        outlet = GameObject.Find(OutletNames.MainMenu).GetComponent<MainMenuOutlet>();
        outlet.button.button.onClick.AddListener(OpenGameplay);
    }

    private void OpenGameplay()
    {
        var gameplay = new GameplayController(LevelType.Level1);
        PushSceneController(gameplay);
    }
}