using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TabHost
{
    private readonly RectTransform tabRoot;
    private readonly Transform viewPort;
    private readonly Func<Enum, TabBaseView> makeView;   // your factory per tab type
    private readonly List<Enum> tabs;
    private readonly TabSystemDirection direction;
    private readonly float spacing;
    private readonly bool enlargeActive;
    private readonly float activeMultiplier;

    private TabSystem tabSystem;
    private UIPagesViewController pageController;
    private readonly List<UIViewController> tabViews = new();
    private readonly UIStackNavigationController stackController;

    public event Action<TabData> OnTabSelected;
    public event Action<TabData> OnTabDeselected;

    public TabHost(
        RectTransform tabRoot,
        Transform viewPort,
        List<Enum> tabs,
        Func<Enum, TabBaseView> makeView,
        UIStackNavigationController stackController,
        TabSystemDirection direction = TabSystemDirection.Horizontal,
        float spacing = 0f,
        bool enlargeActive = true,
        float activeMultiplier = 2f)
    {
        this.tabRoot = tabRoot;
        this.viewPort = viewPort;
        this.tabs = tabs;
        this.makeView = makeView;
        this.stackController = stackController;
        this.direction = direction;
        this.spacing = spacing;
        this.enlargeActive = enlargeActive;
        this.activeMultiplier = Mathf.Max(1f, activeMultiplier);
    }

    public void Build(Enum initialTab)
    {
        var pairs = BuildTabData();
        pageController = new UIPagesViewController(stackController);
        
        tabSystem = new TabSystem(
            parent: tabRoot,
            tabs: tabs,
            tabData: pairs,
            tabSystemDirection: direction,
            offset: spacing,
            tabViewPrefabName: "TabView",
            enlargeActiveTab: enlargeActive,
            activeSizeMultiplier: activeMultiplier,
            animateResize: true,
            resizeDuration: 0.2f
        );

        // Hook selection
        tabSystem.onTabSelected += HandleSelected;
        tabSystem.onTabDeselected += HandleDeselected;

        // Create and present pages once
        foreach (var td in pairs)
        {
            tabViews.Add(td.tabController);
        }
        pageController.SetPages(tabViews);

        // Select initial
        tabSystem.Preselect(initialTab);
    }

    public void ToggleDisabled(Enum type, bool isDisabled)
    {
        tabSystem.ToggleDisabled(type, isDisabled);
    }

    private List<TabData> BuildTabData()
    {
        var list = new List<TabData>(tabs.Count);
        for (int i = 0; i < tabs.Count; i++)
        {
            var t = tabs[i];
            var view = makeView(t);
            list.Add(new TabData
            {
                tabIndex = i,
                tabType = t,
                tabController = view
            });
        }
        return list;
    }

    private void HandleSelected(TabData data)
    {
        ServiceProvider.audioService.PlayOneShot(SoundIds.ui_tab_switch);
        pageController.GoToPage(data.tabIndex);
        OnTabSelected?.Invoke(data);
    }

    private void HandleDeselected(TabData data)
    {
        OnTabDeselected?.Invoke(data);
    }

    public void Cleanup()
    {
        tabSystem.onTabSelected -= HandleSelected;
        tabSystem.onTabDeselected -= HandleDeselected;
        foreach (var tabView in tabViews)
        {
            if (tabView is TabBaseView tbv)
            {
                tbv.Cleanup();
            }
        }
        tabSystem?.Cleanup();
        tabViews.Clear();
    }
}
