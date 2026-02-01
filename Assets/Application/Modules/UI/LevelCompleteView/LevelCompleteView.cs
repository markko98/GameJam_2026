using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class LevelCompleteView : UIViewController
{
    private LevelCompleteViewOutlet outlet;
    private readonly Action onContinueAction;

    private Sprite maskSprite;
    private string maskDescription;

    public LevelCompleteView(Action onContinueAction, Transform viewport, UIStackNavigationController controller, Sprite maskSprite, string maskDescription) : base(controller)
    {
        this.onContinueAction = onContinueAction;
        this.maskSprite = maskSprite;
        this.maskDescription = maskDescription;

        var prefab = Resources.Load<GameObject>(Strings.UIViewsResourcesPath + "LevelCompleteView");
        view = Object.Instantiate(prefab, viewport, false);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        outlet = view.GetComponentInChildren<LevelCompleteViewOutlet>();

        outlet.maskDisplayImage.sprite = maskSprite;
        outlet.descriptionText.text = maskDescription;
    }

    public override void ViewWillAppear()
    {
        outlet.continueButton.button.onClick.AddListener(GoToNextLevel);
    }

    private void GoToNextLevel()
    {
        onContinueAction?.Invoke();
    }


    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        Cleanup();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        outlet.continueButton.button.onClick.RemoveListener(GoToNextLevel);
    }
}
