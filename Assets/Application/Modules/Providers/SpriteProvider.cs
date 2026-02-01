using System;
using UnityEngine;

public class SpriteProvider: MonoBehaviour
{
    private static SpriteProvider _instance;
    public static SpriteProvider Instance
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

        _instance = Resources.Load<SpriteProvider>(Strings.AssetProvidersPath + "SpriteProvider");
        DontDestroyOnLoad(_instance);
    }    
    
    public Sprite mask1Sprite;
    public Sprite mask2Sprite;
    public Sprite mask3Sprite;
    public Sprite mask4Sprite;
    public Sprite lockSprite;


    public static Sprite GetMaskSprite(MaskType type)
    {
        return type switch
        {
            MaskType.Lono => Instance.mask1Sprite,
            MaskType.Kane => Instance.mask2Sprite,
            MaskType.Ku => Instance.mask3Sprite,
            MaskType.Kanaloa => Instance.mask4Sprite,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static Sprite GetLockSprite()
    {
        return Instance.lockSprite;
    }
}