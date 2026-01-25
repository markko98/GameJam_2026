using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class ModalLocator
{
    public static async Task<IModalPresenter> BuildPresenter(
        string resourcesPath,
        ModalTheme theme,
        Transform viewportTarget = null)
    {
        Transform viewport = viewportTarget ?? TryPersistentViewport();
        if (viewport == null) viewport = CreateFallbackCanvas().transform;

        GameObject prefab = Resources.Load<GameObject>(resourcesPath);
        if (prefab == null)
        {
            Debug.LogError($"Modal prefab not found at Resources/{resourcesPath}");
            return null;
        }

        var uiStack = new UIStackNavigationController(viewport);
        var presenter = new StackModalPresenter(uiStack, prefab, theme);
        return presenter;
    }

    private static Transform TryPersistentViewport()
    {
        return PersistentReferences.Instance.fullScreenViewport;
    }

    private static Canvas CreateFallbackCanvas()
    {
        var go = new GameObject("ModalViewport", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1170, 2532);
        scaler.matchWidthOrHeight = 0.5f;
        Object.DontDestroyOnLoad(go);
        return canvas;
    }
}
