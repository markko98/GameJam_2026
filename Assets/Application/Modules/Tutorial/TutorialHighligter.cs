using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialHighlighter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private Image topBlocker;
    [SerializeField] private Image bottomBlocker;
    [SerializeField] private Image leftBlocker;
    [SerializeField] private Image rightBlocker;
    [SerializeField] private RectTransform highlightFrame;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Hole Tap (optional)")]
    [SerializeField] private Button holeTapButton;
    [SerializeField] private Image holeTapVisual;

    [Header("Style")]
    [SerializeField, Min(0f)] private float padding = 16f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f;
    [SerializeField] private bool followTargetEveryFrame = true;

    private Camera uiCamera;
    private RectTransform currentTarget;
    private bool isShown;
    private Action holeTapCallback;

    private void Reset()
    {
        overlayCanvas = GetComponentInParent<Canvas>();
        overlayRoot = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        uiCamera = overlayCanvas != null && overlayCanvas.renderMode == RenderMode.ScreenSpaceCamera
            ? overlayCanvas.worldCamera
            : null;

        if (holeTapButton != null)
        {
            holeTapButton.onClick.RemoveAllListeners();
            holeTapButton.onClick.AddListener(() => holeTapCallback?.Invoke());
        }

        HideInstant();
    }

    private void LateUpdate()
    {
        if (!isShown || currentTarget == null || !followTargetEveryFrame) return;
        UpdateHole(currentTarget);
    }

    public void Show(RectTransform target, Action onHoleTap = null)
    {
        currentTarget = target;
        isShown = true;
        holeTapCallback = onHoleTap;
        gameObject.SetActive(true);

        canvasGroup.DOKill();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, fadeDuration);

        UpdateHole(target);
        ConfigureHoleTapMode();
    }

    public void Hide()
    {
        isShown = false;
        currentTarget = null;
        holeTapCallback = null;
        DisableHoleTap();

        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        });
    }

    public void HideInstant()
    {
        isShown = false;
        currentTarget = null;
        holeTapCallback = null;
        DisableHoleTap();

        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);
    }

    private void ConfigureHoleTapMode()
    {
        if (holeTapButton == null) return;

        if (holeTapCallback != null)
        {
            // Hole is a clickable button (for non-button anchors)
            holeTapButton.gameObject.SetActive(true);
            if (holeTapVisual != null) holeTapVisual.raycastTarget = true;
        }
        else
        {
            DisableHoleTap();
        }
    }
    public void EnableHoleTap(Action onHoleTap)
    {
        holeTapCallback = onHoleTap;
        ConfigureHoleTapMode();
    }
    private void DisableHoleTap()
    {
        if (holeTapButton != null) holeTapButton.gameObject.SetActive(false);
        if (holeTapVisual != null) holeTapVisual.raycastTarget = false;
    }

    private void UpdateHole(RectTransform target)
    {
        var hole = GetHoleRectInOverlaySpace(target);
        hole.xMin -= padding; hole.xMax += padding;
        hole.yMin -= padding; hole.yMax += padding;

        LayoutBlockers(hole);
        LayoutFrame(hole);
        LayoutHoleTap(hole);
    }

    private Rect GetHoleRectInOverlaySpace(RectTransform target)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2[] local = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRoot, sp, uiCamera, out local[i]);
        }

        float xMin = Mathf.Min(local[0].x, local[2].x);
        float xMax = Mathf.Max(local[0].x, local[2].x);
        float yMin = Mathf.Min(local[0].y, local[2].y);
        float yMax = Mathf.Max(local[0].y, local[2].y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private void LayoutBlockers(Rect hole)
    {
        var root = overlayRoot.rect;

        // Top: region above the hole
        SetRectCenter(topBlocker.rectTransform,
            anchoredCenter: new Vector2(0f, (root.yMax + hole.yMax) * 0.5f),
            size: new Vector2(root.width, Mathf.Max(0f, root.yMax - hole.yMax)));

        // Bottom: region below the hole
        SetRectCenter(bottomBlocker.rectTransform,
            anchoredCenter: new Vector2(0f, (root.yMin + hole.yMin) * 0.5f),
            size: new Vector2(root.width, Mathf.Max(0f, hole.yMin - root.yMin)));

        // Left: region left of the hole
        SetRectCenter(leftBlocker.rectTransform,
            anchoredCenter: new Vector2((root.xMin + hole.xMin) * 0.5f, hole.center.y),
            size: new Vector2(Mathf.Max(0f, hole.xMin - root.xMin), hole.height));

        // Right: region right of the hole
        SetRectCenter(rightBlocker.rectTransform,
            anchoredCenter: new Vector2((hole.xMax + root.xMax) * 0.5f, hole.center.y),
            size: new Vector2(Mathf.Max(0f, root.xMax - hole.xMax), hole.height));

        topBlocker.raycastTarget = bottomBlocker.raycastTarget =
            leftBlocker.raycastTarget = rightBlocker.raycastTarget = true;
    }

    private static void SetRectCenter(RectTransform rt, Vector2 anchoredCenter, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredCenter;
        rt.sizeDelta = size;
    }

    private void LayoutFrame(Rect hole)
    {
        if (highlightFrame == null) return;
        highlightFrame.gameObject.SetActive(true);
        highlightFrame.anchoredPosition = hole.center;
        highlightFrame.sizeDelta = hole.size;
    }

    private void LayoutHoleTap(Rect hole)
    {
        if (holeTapButton == null || !holeTapButton.gameObject.activeSelf) return;
        var rt = holeTapButton.transform as RectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = hole.center;
        rt.sizeDelta = hole.size;

        if (holeTapVisual != null)
        {
            var imgRt = holeTapVisual.transform as RectTransform;
            imgRt.anchorMin = imgRt.anchorMax = new Vector2(0.5f, 0.5f);
            imgRt.pivot = new Vector2(0.5f, 0.5f);
            imgRt.anchoredPosition = Vector2.zero;
            imgRt.sizeDelta = hole.size;
        }
    }
}
