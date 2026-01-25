using System;
using Coffee.UIEffects;
using UnityEngine;

public class UIEffectProvider : MonoBehaviour
{
    [SerializeField] private UIEffectCatalogue uiEffectCatalogue;

    private static UIEffectProvider _instance;

    public static UIEffectProvider Instance
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

        _instance = Resources.Load<UIEffectProvider>(Strings.AssetProvidersPath + "UIEffect/UIEffectProvider");
        DontDestroyOnLoad(_instance);
    }
    public static UIEffectPreset GetPresetForButton(bool active)
    {
        return active ? Instance.uiEffectCatalogue.buttonActive : Instance.uiEffectCatalogue.buttonInactive;
    }

    public static UIEffectPreset GetPresetForGrayscaleOrNormal(bool isUnlocked)
    {
        return isUnlocked ? Instance.uiEffectCatalogue.normal : Instance.uiEffectCatalogue.grayscale;
    }
}