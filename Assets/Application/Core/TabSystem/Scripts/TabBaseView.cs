using UnityEngine;
using UnityEngine.UI;

public class TabBaseView : UIViewController, ITabController
{
    // Scroll position memory
    private Vector2? savedScrollPosition;
    protected UIStackNavigationController controller;
    
    public TabBaseView(GameObject view, UIStackNavigationController controller) : base(view, controller)
    {
        this.controller = controller;
    }

    public TabBaseView(UIStackNavigationController controller) : base(controller)
    {
        this.controller = controller;
    }
    
    protected virtual void PreSetup()
    {
    }

    public virtual void Select()
    {
    }

    public virtual void Deselect()
    {
    }

    protected void SaveScrollPosition()
    {
    }

    protected void SetScrollPosition()
    {
    }

    public virtual void Cleanup()
    {
        
    }
}