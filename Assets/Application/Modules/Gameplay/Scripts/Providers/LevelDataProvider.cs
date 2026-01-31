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
}

public struct LevelData
{
    public LevelType leveType;
    public LevelSO levelDataSO;
}