using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TabAssetProvider : MonoBehaviour
{
    
    private static TabAssetProvider _instance;
    public static TabAssetProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                Prewarm();
            }

            return _instance;
        }
    }
    
    public static void Prewarm()
    {
        if (_instance != null)
        {
            return;
        }

        _instance = Resources.Load<TabAssetProvider>(Strings.AssetProvidersPath + "TabAssetProvider");
        DontDestroyOnLoad(_instance);
    }
    public static string GetTabDescription(Enum tabType)
    {
        if (tabType is TabType)
        {
            return tabType switch
            {
                TabType.Tab1 => "tab 1",
                _ => ""
            };
        }

        return "";
    }

    public static string GetTutorialAnchorIdFor(Enum tabType)
    {
        if (tabType is TabType)
        {
            return tabType switch
            {
                TabType.Tab1 => TutorialAnchorIds.MainMenuTabHangar,
                _ => ""
            };
        }

        return "";
    }
}