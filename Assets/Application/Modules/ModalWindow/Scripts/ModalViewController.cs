using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ModalViewController : UIViewController
{
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI bodyText;
    private Transform iconHolder;
    private Image icon;
    private ButtonView confirmButton;
    private ButtonView closeButton;
    private RectTransform window;

    private Action<ModalResult> completion;
    private ModalRequest req;

    public ModalViewController(GameObject view, UIStackNavigationController controller) : base(view, controller)
    {
    }

    public void Setup(ModalRequest req, ModalTheme theme, Action<ModalResult> completion)
    {
        this.req = req;
        this.completion = completion;
        animationType = req.animationShowOverride ?? AnimationType.ScaleUpFromMiddle;

        CacheRefs();
        ApplyTheme(theme);
        ApplyRequest(req);
    }

    private void CacheRefs()
    {
        titleText = GetTextWithName("TitleText");
        progressText = GetTextWithName("ProgressText");
        bodyText = GetTextWithName("BodyText");
        iconHolder = GetUIViewComponentWithName("IconHolder").transform;
        window = GetUIViewComponentWithName("Window").GetComponent<RectTransform>();
        icon = GetUIImageViewComponentWithName("ModalIcon")?.GetComponent<Image>()
               ?? view.GetComponentInChildren<Image>(true);
        confirmButton = GetUIViewComponentWithName("UI_Generic_Button_Confirm").GetComponent<ButtonView>();
        closeButton = GetUIViewComponentWithName("UI_Generic_Button_Close").GetComponent<ButtonView>();
    }

    private void ApplyTheme(ModalTheme theme)
    {
        if (theme == null) return;
        if (titleText) titleText.color = theme.titleColor;
        if (bodyText) bodyText.color = theme.bodyColor;
    }

    private void ApplyRequest(ModalRequest r)
    {
        if (titleText) titleText.text = r.title ?? "";
        if (progressText) progressText.text = r.progress ?? "";
        if (bodyText) bodyText.text = r.description ?? "";

        if (icon != null)
        {
            Sprite sp = r.sprite;
            iconHolder.gameObject.SetActive(sp != null);
            icon.sprite = sp;
            iconHolder.GetComponent<RectTransform>().sizeDelta = r.spriteSize;
            icon.rectTransform.sizeDelta = r.spriteSize;
            icon.gameObject.SetActive(sp != null);
        }

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(r.showConfirm);
            confirmButton.text.SetText(r.confirmLabel);
            confirmButton.button.onClick.RemoveAllListeners();
            confirmButton.button.onClick.AddListener(() => Finish(ModalResult.Confirmed));
        }

        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(r.showClose);
            closeButton.text.SetText(r.closeLabel);
            closeButton.button.onClick.RemoveAllListeners();
            closeButton.button.onClick.AddListener(() => Finish(ModalResult.Closed));
        }
    }

    public void CloseExternally(ModalResult result)
    {
        Finish(result);
    }

    private void Finish(ModalResult result)
    {
        RemovePopUp(0f, () => { completion?.Invoke(result); },
            req.animationHideOverride ?? AnimationType.ScaleDownFromMiddle);
    }

    private void PlaceNearTarget(
        Transform viewportTarget,
        Rect targetRectInViewport,
        ModalPlacement placement,
        Vector2 offset,
        bool clampToViewport,
        float padding,
        float gap = 16f)
    {
        var viewport = viewportTarget;
        if (viewport == null || window == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(window);

        var size = new Vector2(
            Mathf.Max(LayoutUtility.GetPreferredSize(window, 0), window.rect.width),
            Mathf.Max(LayoutUtility.GetPreferredSize(window, 1), window.rect.height)
        );

        var vpRect = viewport.GetComponent<RectTransform>().rect;
        var chosen = (placement == ModalPlacement.Auto)
            ? ChooseAutoSide(vpRect, targetRectInViewport, size, gap, padding)
            : placement;

        SetPivotForSide(window, chosen);

        var pos = ComputePosForSide(chosen, targetRectInViewport, size, gap);
        pos += offset;

        if (clampToViewport)
            pos = ClampInsideViewport(pos, window.pivot, size, vpRect, padding, targetRectInViewport, gap, chosen);

        window.anchorMin = window.anchorMax = new Vector2(0.5f, 0.5f);
        window.anchoredPosition = pos;
    }

    private ModalPlacement ChooseAutoSide(Rect vp, Rect target, Vector2 wSize, float gap, float pad)
    {
        float top = vp.yMax - (target.yMax + gap + wSize.y);
        float bottom = (target.yMin - gap - wSize.y) - vp.yMin;
        float right = vp.xMax - (target.xMax + gap + wSize.x);
        float left = (target.xMin - gap - wSize.x) - vp.xMin;

        (float s, ModalPlacement side)[] c =
        {
            (top, ModalPlacement.Above),
            (bottom, ModalPlacement.Below),
            (right, ModalPlacement.Right),
            (left, ModalPlacement.Left)
        };
        var best = c[0];
        for (int i = 1; i < c.Length; i++)
            if (c[i].s > best.s)
                best = c[i];
        return best.side;
    }

    private void SetPivotForSide(RectTransform rt, ModalPlacement side)
    {
        switch (side)
        {
            case ModalPlacement.Above:
                rt.pivot = new Vector2(0.5f, 0f);
                break;
            case ModalPlacement.Below:
                rt.pivot = new Vector2(0.5f, 1f);
                break;
            case ModalPlacement.Left:
                rt.pivot = new Vector2(1f, 0.5f);
                break;
            case ModalPlacement.Right:
                rt.pivot = new Vector2(0f, 0.5f);
                break;
            default:
                rt.pivot = new Vector2(0.5f, 0.5f);
                break;
        }
    }

    private Vector2 ComputePosForSide(ModalPlacement side, Rect target, Vector2 size, float gap)
    {
        return side switch
        {
            ModalPlacement.Above => new Vector2(target.center.x, target.yMax + gap),
            ModalPlacement.Below => new Vector2(target.center.x, target.yMin - gap),
            ModalPlacement.Left => new Vector2(target.xMin - gap, target.center.y),
            ModalPlacement.Right => new Vector2(target.xMax + gap, target.center.y),
            _ => Vector2.zero
        };
    }

    private Vector2 ClampInsideViewport(
        Vector2 pos, Vector2 pivot, Vector2 size, Rect vp,
        float pad, Rect keepOut, float gap, ModalPlacement side)
    {
        float left = size.x * pivot.x;
        float right = size.x * (1f - pivot.x);
        float down = size.y * pivot.y;
        float up = size.y * (1f - pivot.y);

        float minX = vp.xMin + pad + left;
        float maxX = vp.xMax - pad - right;
        float minY = vp.yMin + pad + down;
        float maxY = vp.yMax - pad - up;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // keep a clear gap so we never cover the target
        switch (side)
        {
            case ModalPlacement.Above:
                float bottom = pos.y - down;
                float needBottom = keepOut.yMax + gap;
                if (bottom < needBottom) pos.y += (needBottom - bottom);
                break;
            case ModalPlacement.Below:
                float top = pos.y + up;
                float needTop = keepOut.yMin - gap;
                if (top > needTop) pos.y -= (top - needTop);
                break;
            case ModalPlacement.Left:
                float rightEdge = pos.x + right;
                float needRight = keepOut.xMin - gap;
                if (rightEdge > needRight) pos.x -= (rightEdge - needRight);
                break;
            case ModalPlacement.Right:
                float leftEdge = pos.x - left;
                float needLeft = keepOut.xMax + gap;
                if (leftEdge < needLeft) pos.x += (needLeft - leftEdge);
                break;
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

    // placement
    public void PlaceByMode(
        RectTransform viewport,
        ModalRequest req,
        float gapForAuto = 16f)
    {
        if (viewport == null || window == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(window);

        switch (req.positionMode)
        {
            case ModalPositionMode.AutoNearTarget:
                if (req.targetRectInViewport.HasValue)
                {
                    PlaceNearTarget(
                        viewport,
                        req.targetRectInViewport.Value,
                        req.placement,
                        req.placementOffset,
                        req.clampToViewport,
                        req.viewportPadding,
                        gapForAuto);
                }
                else
                {
                    // Fallback if no target rect – center it
                    PlaceAtViewportPoint(viewport, GetViewportCenter(viewport), req);
                }

                break;

            case ModalPositionMode.Center:
                PlaceAtViewportPoint(viewport, GetViewportCenter(viewport), req);
                break;

            case ModalPositionMode.ViewportNormalized:
            {
                var r = viewport.rect;
                var p = new Vector2(
                    Mathf.Lerp(r.xMin, r.xMax, Mathf.Clamp01(req.position.x)),
                    Mathf.Lerp(r.yMin, r.yMax, Mathf.Clamp01(req.position.y))
                );
                PlaceAtViewportPoint(viewport, p, req);
                break;
            }

            case ModalPositionMode.ViewportPixels:
                PlaceAtViewportPoint(viewport, req.position, req);
                break;
        }
    }

    private static Vector2 GetViewportCenter(RectTransform viewport)
    {
        var r = viewport.rect;
        return new Vector2((r.xMin + r.xMax) * 0.5f, (r.yMin + r.yMax) * 0.5f);
    }

    /// <summary>
    /// Places the Window so its pivot sits at 'point' (viewport local space),
    /// applies clamp and optional pivot override.
    /// </summary>
    private void PlaceAtViewportPoint(RectTransform viewport, Vector2 point, ModalRequest req)
    {
        if (viewport == null || window == null) return;

        // pivot: use override or sensible default
        if (req.pivotOverride.HasValue)
            window.pivot = req.pivotOverride.Value;
        else
            window.pivot = (req.positionMode == ModalPositionMode.Center)
                ? new Vector2(0.5f, 0.5f)
                : window.pivot; // keep existing

        // measure size after layout
        var size = new Vector2(
            Mathf.Max(LayoutUtility.GetPreferredSize(window, 0), window.rect.width),
            Mathf.Max(LayoutUtility.GetPreferredSize(window, 1), window.rect.height)
        );

        // set base pos
        window.anchorMin = window.anchorMax = new Vector2(0.5f, 0.5f);
        var pos = point;

        // clamp fully on screen if requested
        if (req.clampToViewport)
        {
            var r = viewport.rect;
            float left = size.x * window.pivot.x;
            float right = size.x * (1f - window.pivot.x);
            float down = size.y * window.pivot.y;
            float up = size.y * (1f - window.pivot.y);

            float minX = r.xMin + req.viewportPadding + left;
            float maxX = r.xMax - req.viewportPadding - right;
            float minY = r.yMin + req.viewportPadding + down;
            float maxY = r.yMax - req.viewportPadding - up;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        window.anchoredPosition = pos;
    }

}