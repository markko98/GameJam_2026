using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataProvider : MonoBehaviour
{
    public List<LevelData> LevelDatas = new();
    public LevelSO backupLevelSO;

    private static LevelDataProvider _instance;

    public static LevelDataProvider Instance
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

        _instance = Resources.Load<LevelDataProvider>(Strings.AssetProvidersPath + "LevelDataProvider");
        DontDestroyOnLoad(_instance);
    }


    public static LevelSO GetLevelData(LevelType levelType)
    {
        return Instance.LevelDatas.Find(x => x.leveType == levelType).levelDataSO ?? Instance.backupLevelSO;
    }

    public static string GetMaskDescription(MaskType mask)
    {
        return mask switch
        {            
            MaskType.None => "",
            MaskType.Kane => "The Mask of Kane pulses with the warmth of a newborn sun, turning the world into a golden haze. With its power, you shall walk through stone like morning mist and tread upon traps as if they were solid light.",
            MaskType.Lono => "The Mask of Lono radiates a gentle, emerald hum, causing the air to smell of fresh rain and growth. With its blessing, you shall mend shattered timber bridges with a touch and command wild jungle vines to weave into sturdy new paths.",
            MaskType.Ku => "The Mask of Ku thrums with the iron-willed heat of a thousand battles. Hardened by an unbreakable spirit, you shall shrug off stinging arrow volleys and stride over lethal floor spikes without drawing a drop of blood.",
            MaskType.Kanaloa => "The Mask of Kanaloa surges with the cool, rhythmic power of the deep tide. Its icy aura chills molten lava into harmless stone at your touch, allowing you to cross volcanic pits as if they were cooling reefs.",
            _ => ""
        };
    }
}

[Serializable]
public struct LevelData
{
    public LevelType leveType;
    public LevelSO levelDataSO;
}