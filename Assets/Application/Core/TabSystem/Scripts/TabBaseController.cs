using UnityEngine;
using UnityEngine.UI;

public class TabBaseController : USceneController, ITabController
{
    private TabBaseControllerOutlet outlet;
    protected bool shouldBeActiveOnLoad { get; set; }
    protected bool initialized;

    // Scroll position memory
    private Vector2? savedScrollPosition;

    protected TabBaseController(string sceneName) : base(sceneName) { }

    protected TabBaseControllerOutlet GetOutlet(GameObject outletObject)
    {
        outlet = outletObject.GetComponent<TabBaseControllerOutlet>();
        return outlet;
    }

    protected virtual void PreSetup()
    {
        if (shouldBeActiveOnLoad)
        {
            Select();
        }
        else
        {
            Deselect();
        }
    }

    public virtual void Select()
    {
        if (IsLoaded == false)
        {
            shouldBeActiveOnLoad = true;
            return;
        }

        outlet?.tabRoot.SetActive(true);
    }

    public virtual void Deselect()
    {
        if (IsLoaded == false)
        {
            shouldBeActiveOnLoad = false;
            return;
        }
        
        outlet?.tabRoot.SetActive(false);
    }

    protected void SaveScrollPosition()
    {
        if (outlet?.scrollRect)
        {
            PlayerPrefs.SetFloat(SceneName + "_ScrollPosition", outlet.scrollRect.normalizedPosition.y);
            PlayerPrefs.Save();
        }
    }

    protected void SetScrollPosition()
    {
        if (outlet?.scrollRect)
        {
            var yPos = PlayerPrefs.GetFloat(SceneName + "_ScrollPosition", 1);
            Vector2 scrollPosition = new Vector2(0, yPos);
            outlet.scrollRect.normalizedPosition = scrollPosition;
        }
    }
}
