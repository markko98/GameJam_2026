using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum TabType
{
    Tab1,
}

public enum TabSystemDirection
{
    Horizontal,
    Vertical,
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class TabSystem
{
    // ------- Config -------
    private readonly RectTransform parent;
    private readonly List<Enum> tabsToShow;
    private readonly TabSystemDirection tabSystemDirection;
    private readonly float offset;
    private readonly string tabViewPrefabName;

    // Enlarge options
    private readonly bool enlargeActiveTab;
    private readonly float activeSizeMultiplier;
    private readonly bool animateResize;
    private readonly float resizeDuration;
    private readonly Ease resizeEase;
    private readonly bool bleedToScreenEdges;

    // ------- State -------
    private readonly List<TabEntry> entries = new();
    private readonly List<TabData> tabDataList;
    private GameObject tabViewPrefab;

    private Vector2 baseTabSize;
    private bool didPreselect = false;

    public Enum activeTab { get; private set; }
    public Action<TabData> onTabSelected;
    public Action<TabData> onTabDeselected;

    private Sequence showTabRootSequence;
    private Sequence hideTabRootSequence;

    /// <param name="enlargeActiveTab">If true, active tab is larger along the main axis.</param>
    /// <param name="activeSizeMultiplier">2.0 = double size of active tab.</param>
    /// <param name="animateResize">If true, smoothly tween size changes.</param>
    /// <param name="resizeDuration">Tween duration.</param>
    /// <param name="resizeEase">Tween ease.</param>
    /// <param name="bleedToScreenEdges">Expand size to screen edge</param>
    public TabSystem(
        RectTransform parent,
        List<Enum> tabs,
        List<TabData> tabData,
        TabSystemDirection tabSystemDirection = TabSystemDirection.Horizontal,
        float offset = 0f,
        string tabViewPrefabName = "TabView",
        bool enlargeActiveTab = true,
        float activeSizeMultiplier = 1.75f,
        bool animateResize = true,
        float resizeDuration = 0.2f,
        Ease resizeEase = Ease.OutQuad,
        bool bleedToScreenEdges = true
    )
    {
        this.parent = parent;
        this.tabsToShow = tabs ?? new List<Enum>();
        this.offset = offset;
        this.tabSystemDirection = tabSystemDirection;
        this.tabDataList = new List<TabData>(tabData ?? new List<TabData>());
        this.tabViewPrefabName = tabViewPrefabName;

        this.enlargeActiveTab = enlargeActiveTab;
        this.activeSizeMultiplier = Mathf.Max(1f, activeSizeMultiplier);
        this.animateResize = animateResize;
        this.resizeDuration = Mathf.Max(0f, resizeDuration);
        this.resizeEase = resizeEase;
        this.bleedToScreenEdges = bleedToScreenEdges;

        tabViewPrefab = Resources.Load<GameObject>(Strings.UISharedResourcesPath + tabViewPrefabName);

        CreateTabViews(this.tabsToShow);

        CalculateBaseTabSize(this.enlargeActiveTab);
        
        HideAllTabRoots();
    }

    public void Cleanup()
    {
        foreach (var e in entries)
        {
            e.view.onTabSelected -= (x)=>
            {
                ChangeTab(x);
            };
            e.view.Cleanup();
        }
        entries.Clear();
    }

    public void ToggleDisabled(Enum tabType, bool isDisabled)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var tab = entries.Find(x => Equals(x.id, tabType));
            if (tab != null)
            {
                tab.view.ToggleDisabled(isDisabled);
            }
        }
    }

    public void Preselect(Enum tabType)
    {
        ChangeTab(tabType, instant: false);
        didPreselect = true;

        DelayedExecutionManager.ExecuteActionAfterDelay(100,()=>
        {
            Reflow(GetEntryIndexById(tabType), false);
        });
    }

    public void ChangeTab(Enum tabType, bool instant = false)
    {
        if (!didPreselect)
        {
            activeTab = tabType;
        }

        if (Equals(activeTab, tabType) && didPreselect) return;

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            bool isActive = Equals(e.id, tabType);
            if(e.view.IsDisabled) return; // dont select if disabled
            if (isActive)
            {
                e.view.Select();
                activeTab = tabType;
                ChangeTabRoot(e.id);
            }
            else
            {
                e.view.Deselect();
            }
        }

        if (didPreselect)
        {
            Reflow(GetEntryIndexById(tabType), false);
        }
    }

    private void CreateTabViews(List<Enum> tabTypes)
    {
        entries.Clear();

        foreach (var id in tabTypes)
        {
            var view = new TabView(tabViewPrefab, parent, id);
            view.onTabSelected += (x)=>
            {
                ChangeTab(x);
            };
            entries.Add(new TabEntry { id = id, view = view });
        }
    }

    /// <summary>
    /// Solve baseTabSize so total width/height fits exactly when one tab is enlarged by multiplier.
    /// Horizontal:  sum = baseW * ((N-1) + M) + offset*(N-1) = parentWidth  => baseW = (parentWidth - offset*(N-1)) / ((N-1)+M)
    /// Vertical:    sum = baseH * ((N-1) + M) + offset*(N-1) = parentHeight => baseH = (parentHeight - offset*(N-1)) / ((N-1)+M)
    /// If not enlarging, M = 1.
    /// </summary>
    private void CalculateBaseTabSize(bool includeActiveMultiplier)
    {
        var n = Mathf.Max(1, entries.Count);
        var m = includeActiveMultiplier ? activeSizeMultiplier : 1f;
        var gaps = offset * (n - 1);

        var rect = parent.rect;

        if (tabSystemDirection == TabSystemDirection.Horizontal)
        {
            var available = Mathf.Max(1f, rect.width - gaps);
            var denom = Mathf.Max(1f, (n - 1) + m);
            var baseW = available / denom;
            var baseH = Mathf.Max(1f, rect.height);

            baseTabSize = new Vector2(baseW, baseH);
        }
        else
        {
            var available = Mathf.Max(1f, rect.height - gaps);
            var denom = Mathf.Max(1f, (n - 1) + m);
            var baseH = available / denom;
            var baseW = Mathf.Max(1f, rect.width);

            baseTabSize = new Vector2(baseW, baseH);
        }
    }

    /// <summary>
    /// Build per-tab sizes (1× or M× for the active), then center and place them along the main axis.
    /// </summary>
    private void Reflow(int? activeIndexOverride, bool instant)
    {
        if (entries.Count == 0) return;

        var activeIndex = activeIndexOverride ?? GetEntryIndexById(activeTab);
        if (activeIndex < 0) activeIndex = 0; // default to first

        // Compute per-tab sizes
        var sizes = new List<Vector2>(entries.Count);
        
        float bleedUnits = 0f;
        if (bleedToScreenEdges)
        {
            bleedUnits = PixelsToParentUnits(GetBleedPixels());
        }
        for (var i = 0; i < entries.Count; i++)
        {
            var isActive = (enlargeActiveTab && i == activeIndex);
            if (tabSystemDirection == TabSystemDirection.Horizontal)
            {
                var w = baseTabSize.x * (isActive ? activeSizeMultiplier : 1f);
                sizes.Add(new Vector2(w, baseTabSize.y + bleedUnits));
            }
            else
            {
                var h = baseTabSize.y * (isActive ? activeSizeMultiplier : 1f);
                sizes.Add(new Vector2(baseTabSize.x + bleedUnits, h));
            }
        }

        // Compute total span along main axis, including gaps
        var total = 0f;
        for (var i = 0; i < sizes.Count; i++)
        {
            total += (tabSystemDirection == TabSystemDirection.Horizontal) ? sizes[i].x : sizes[i].y;
        }
        total += offset * (entries.Count - 1);

        // Start centered
        var start = -total * 0.5f;
        var cursor = start;

        for (var i = 0; i < entries.Count; i++)
        {
            var size = sizes[i];
            var centerOffset = (tabSystemDirection == TabSystemDirection.Horizontal) ? size.x * 0.5f : size.y * 0.5f;

            var pos = (tabSystemDirection == TabSystemDirection.Horizontal)
                ? new Vector2(cursor + centerOffset, 0f)
                : new Vector2(0f, cursor + centerOffset);

            entries[i].view.SetSize(size, animateResize && !instant, resizeDuration, resizeEase);
            entries[i].view.SetPosition(pos, animateResize && !instant, resizeDuration, resizeEase);

            cursor += centerOffset * 2f + offset;
        }
    }

    private void HideAllTabRoots()
    {
        foreach (var td in tabDataList)
            td.tabController?.Deselect();
    }

    private void ChangeTabRoot(Enum newTab)
    {
        var activeTabData = GetTabData(activeTab);
        var newTabData = GetTabData(newTab);

        var activeTabIndex = GetTabIndex(activeTabData);
        var newTabIndex = GetTabIndex(newTabData);

        Direction hideDirection;
        Direction showDirection;

        if (activeTabIndex >= 0 && newTabIndex >= 0 && activeTabIndex > newTabIndex)
        {
            hideDirection = tabSystemDirection == TabSystemDirection.Horizontal ? Direction.Right : Direction.Up;
            showDirection = tabSystemDirection == TabSystemDirection.Horizontal ? Direction.Right : Direction.Up;
        }
        else
        {
            hideDirection = tabSystemDirection == TabSystemDirection.Horizontal ? Direction.Left : Direction.Down;
            showDirection = tabSystemDirection == TabSystemDirection.Horizontal ? Direction.Left : Direction.Down;
        }

        if (didPreselect && activeTabData != null)
            HideTabRoot(activeTabData, hideDirection);

        if (newTabData != null)
            ShowTabRoot(newTabData, showDirection);
    }

    private void ShowTabRoot(TabData tabData, Direction dir)
    {
        tabData.tabController?.Select();
        onTabSelected?.Invoke(tabData);
    }

    private void HideTabRoot(TabData tabData, Direction dir)
    {
        tabData.tabController?.Deselect();
        onTabDeselected?.Invoke(tabData);
    }

    private TabData GetTabData(Enum tabType)
    {
        return tabDataList.Find(x => x.tabType.Equals(tabType));
    }

    private int GetTabIndex(TabData data)
    {
        return data == null ? -1 : tabDataList.IndexOf(data);
    }

    private int GetEntryIndexById(Enum id)
    {
        for (int i = 0; i < entries.Count; i++)
            if (Equals(entries[i].id, id))
                return i;
        return -1;
    }

    private float GetBleedPixels()
    {
        var sa = Screen.safeArea;
        if (tabSystemDirection == TabSystemDirection.Horizontal)
        {
            float bottom = sa.yMin;
            float top = Screen.height - sa.yMax;
            return bottom + top;
        }
        else
        {
            float left = sa.xMin;
            float right = Screen.width - sa.xMax;
            return left + right;
        }
    }

    // Convert pixels -> parent-rect units (works with CanvasScaler)
    private float PixelsToParentUnits(float pixels)
    {
        var rootCanvas = parent.GetComponentInParent<Canvas>();
        var rootRect   = rootCanvas.GetComponent<RectTransform>().rect;

        if (tabSystemDirection == TabSystemDirection.Horizontal)
            return pixels * (rootRect.height / Screen.height);
        else
            return pixels * (rootRect.width / Screen.width);
    }
    private float GetOffsetDistance(Direction direction, Vector2 tabSize)
    {
        return direction is Direction.Down or Direction.Up ? tabSize.y : tabSize.x;
    }

    private class TabEntry
    {
        public Enum id;
        public TabView view;
    }
}

[Serializable]
public class TabData
{
    public int tabIndex;
    public Enum tabType;
    public TabBaseView tabController;
}
