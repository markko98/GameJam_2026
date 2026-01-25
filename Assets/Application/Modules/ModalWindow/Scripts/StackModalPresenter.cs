using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class StackModalPresenter : IModalPresenter
{
    private readonly UIStackNavigationController nav;
    private readonly GameObject prefab;
    private readonly ModalTheme theme;

    private readonly Dictionary<string, ModalTutorialData> byId = new();

    private string lastId;

    public StackModalPresenter(UIStackNavigationController nav, GameObject prefab, ModalTheme theme)
    {
        this.nav = nav;
        this.prefab = prefab;
        this.theme = theme;
    }

    public Task<ModalResult> Present(ModalRequest req)
    {
        // ensure id
        var id = string.IsNullOrEmpty(req.correlationId) ? System.Guid.NewGuid().ToString("N") : req.correlationId;
        req.correlationId = id;

        if (!string.IsNullOrEmpty(lastId)) DismissActive();

        var tcs = new TaskCompletionSource<ModalResult>();
        var parent = nav.Viewport as RectTransform;
        if (prefab == null || parent == null)
        {
            Debug.LogError("[Modal] Missing prefab or parent");
            tcs.TrySetResult(ModalResult.None);
            return tcs.Task;
        }

        var view = Object.Instantiate(prefab, parent, false);
        var vc = new ModalViewController(view, nav);

        byId[id] = new ModalTutorialData(vc, view, tcs);
        lastId = id;

        vc.Setup(req, theme, result =>
        {
            if (byId.TryGetValue(id, out var entry))
            {
                entry.tcs.TrySetResult(result);
                if (entry.go) Object.Destroy(entry.go);
                byId.Remove(id);
                if (lastId == id) lastId = null;
            }
        });

        var viewportRT = parent;
        vc.PlaceByMode(viewportRT, req);

        vc.PresentViewAsPopup(req.showBackgroundBlur, 0f,
            req.animationShowOverride ?? AnimationType.ScaleUpFromMiddle);
        
        return tcs.Task;
    }

    public void DismissActive(ModalResult reason = ModalResult.DismissedBackground)
    {
        if (string.IsNullOrEmpty(lastId)) return;
        DismissById(lastId, reason);
    }

    public void DismissById(string correlationId, ModalResult reason = ModalResult.DismissedBackground)
    {
        if (string.IsNullOrEmpty(correlationId)) return;

        if (!byId.TryGetValue(correlationId, out var entry)) return;
        
        entry.vc?.CloseExternally(reason);
        entry.tcs?.TrySetResult(reason);
        if (entry.go) Object.Destroy(entry.go);
        byId.Remove(correlationId);
        if (lastId == correlationId) lastId = null;
    }
}