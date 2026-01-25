using System;
using UnityEngine;

public class ColorProvider : MonoBehaviour
{
    [SerializeField] private ColorCatalogue colorCatalogue;

    private static ColorProvider _instance;

    public static ColorProvider Instance
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

        _instance = Resources.Load<ColorProvider>(Strings.AssetProvidersPath + "Color/ColorProvider");
        DontDestroyOnLoad(_instance);
    }
    
    public static Color GetTabTextColor(bool selected, bool isDisabled)
    {
        return isDisabled ? Instance.colorCatalogue.tabDisabledColor : selected ? Instance.colorCatalogue.tabActiveColor : Instance.colorCatalogue.tabInactiveColor;
    }
    public static UIGradient GetGrayscaleGradientForButton()
    {
        return Instance.colorCatalogue.grayscaledButtonColors;
    }
}