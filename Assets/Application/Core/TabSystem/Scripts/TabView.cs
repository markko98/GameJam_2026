using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TabView
{
    private readonly GameObject prefab;
    private readonly Enum tabType;
    private readonly Transform parent;

    private RectTransform rect;
    private TabViewOutlet outlet;

    public Action<Enum> onTabSelected;

    private Sequence selectSequence;
    private Sequence deselectSequence;
    private Sequence disabledSequence;

    private float selectAnimationDuration = 0.35f;
    private Vector3 defaultIconScale;
    private Vector3 selectedIconScale;
    private float scaleIncrease = 1.3f;
    private Color inactiveColor;
    private Color activeColor;
    private Color disabledColor;

    public bool IsActive { get; private set; }
    public bool IsDisabled { get; private set; }

    public TabView(GameObject prefab, Transform parent, Enum tabType)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.tabType = tabType;

        Generate();
        CacheVisualDefaults();
    }

    private void Generate()
    {
        rect = GameObject.Instantiate(prefab, parent).GetComponent<RectTransform>();
        outlet = rect.GetComponent<TabViewOutlet>();
        // activeColor = ColorProvider.GetTabTextColor(true, false); 
        // inactiveColor = ColorProvider.GetTabTextColor(false, false);
        // disabledColor = ColorProvider.GetTabTextColor(false, true);
        outlet.tabViewText.color = IsDisabled ? disabledColor : inactiveColor;
        outlet.borderIcon.color = IsDisabled ? disabledColor : inactiveColor;;
        outlet.tabViewText.SetText(TabAssetProvider.GetTabDescription(tabType));
        outlet.tutorialAnchor?.RegisterWithId(TabAssetProvider.GetTutorialAnchorIdFor(tabType)); 
        outlet.tabViewButton.onClick.AddListener(OnClick);
        UpdateView();
    }

    private void CacheVisualDefaults()
    {
        defaultIconScale = outlet.tabViewIcon.transform.localScale;
        selectedIconScale = scaleIncrease * defaultIconScale;
    }

    public void ToggleDisabled(bool isDisabled)
    {
        IsDisabled = isDisabled;
        selectSequence?.Kill(true);
        deselectSequence?.Kill(true);
        disabledSequence?.Kill(true);

        disabledSequence = DOTween.Sequence();
        disabledSequence.Join(outlet.tabViewIcon.transform.DOScale(selectedIconScale, selectAnimationDuration).SetEase(Ease.OutBack));
        disabledSequence.Join(outlet.tabViewText.DOColor(IsDisabled ? disabledColor : IsActive ? activeColor : inactiveColor, selectAnimationDuration).SetEase(Ease.OutQuad)); 
        disabledSequence.Join(outlet.borderIcon.DOColor(IsDisabled ? disabledColor : IsActive ? activeColor : inactiveColor, selectAnimationDuration).SetEase(Ease.OutQuad));

        UpdateView();
    }

    private void UpdateView()
    {
        // outlet.tabViewIcon.sprite = SpriteProvider.GetTabSprite(tabType, IsActive, IsDisabled);
        // outlet.tabViewRootImage.sprite = SpriteProvider.GetTabBackgroundSprite(IsActive, IsDisabled);
    }
    public void Select()
    {
        if (IsDisabled) return;

        IsActive = true;
        selectSequence?.Kill(true);
        deselectSequence?.Kill(true);
        disabledSequence?.Kill(true);

        selectSequence = DOTween.Sequence();
        selectSequence.Join(outlet.tabViewIcon.transform.DOScale(selectedIconScale, selectAnimationDuration).SetEase(Ease.OutBack));
        selectSequence.Join(outlet.tabViewText.DOColor(IsDisabled ? disabledColor : IsActive ? activeColor : inactiveColor, selectAnimationDuration).SetEase(Ease.OutQuad)); 
        selectSequence.Join(outlet.borderIcon.DOColor(IsDisabled ? disabledColor : IsActive ? activeColor : inactiveColor, selectAnimationDuration).SetEase(Ease.OutQuad));
        UpdateView();
    }

    public void Deselect()
    {
        IsActive = false;
        selectSequence?.Kill(true);
        deselectSequence?.Kill(true);
        disabledSequence?.Kill(true);

        deselectSequence = DOTween.Sequence();
        deselectSequence.Join(outlet.tabViewIcon.transform.DOScale(defaultIconScale, selectAnimationDuration).SetEase(Ease.OutQuad));
        deselectSequence.Join(outlet.tabViewText.DOColor(IsDisabled ? disabledColor : IsActive ? activeColor : inactiveColor, selectAnimationDuration).SetEase(Ease.OutQuad));
        deselectSequence.Join(outlet.borderIcon.DOColor(IsDisabled ? disabledColor : IsActive ? activeColor : inactiveColor, selectAnimationDuration).SetEase(Ease.OutQuad));
        UpdateView();
    }

    private void OnClick() => onTabSelected?.Invoke(tabType);

    // ---- Layout hooks used by TabSystem ----
    public void SetPosition(Vector2 anchoredPos, bool animate, float duration, Ease ease)
    {
        if (!animate)
        {
            rect.anchoredPosition = anchoredPos;
            return;
        }

        rect.DOAnchorPos(anchoredPos, duration).SetEase(ease);
    }

    public void SetSize(Vector2 size, bool animate, float duration, Ease ease)
    {
        if (!animate)
        {
            rect.sizeDelta = size;
            return;
        }

        // Tween both axes to preserve layout feel
        DOTween.Kill(rect, false);
        DOTween.To(() => rect.sizeDelta, v => rect.sizeDelta = v, size, duration)
               .SetEase(ease)
               .SetTarget(rect);
    }

    public void Cleanup()
    {
        outlet.tabViewButton.onClick.RemoveListener(OnClick);
        if (rect != null && rect.gameObject != null)
        {
            GameObject.Destroy(rect.gameObject);
        }
    }
}
