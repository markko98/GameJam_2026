using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class TabViewPlaceholder : TabBaseView
{
    private readonly Transform viewport;
    private const string subtitleLabel = "Subtitle";
    private const string titleLabel = "Title";
    
    public TabViewPlaceholder(Transform viewport, UIStackNavigationController controller) : base(controller)
    {
        view =  Object.Instantiate(Resources.Load(Strings.UIResourcesPath + "Tabs/TabViewPlaceholder"), viewport, false) as GameObject;
        this.viewport = viewport;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        GetTextWithName(titleLabel).SetText("Placeholder text title");
        GetTextWithName(subtitleLabel).SetText("Placeholder text subtitle");
    }

}