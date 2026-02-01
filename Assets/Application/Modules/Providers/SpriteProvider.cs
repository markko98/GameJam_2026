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
    
    public Sprite kaneSprite;
    public Sprite lonoSprite;
    public Sprite kuSprite;
    public Sprite kanaloaSprite;
    public Sprite lockSprite;


    public static Sprite GetMaskSprite(MaskType type)
    {
        return type switch
        {
            MaskType.Kane => Instance.kaneSprite,
            MaskType.Lono => Instance.lonoSprite,
            MaskType.Ku => Instance.kuSprite,
            MaskType.Kanaloa => Instance.kanaloaSprite,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static Sprite GetLockSprite()
    {
        return Instance.lockSprite;
    }
}