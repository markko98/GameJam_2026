using System;
using UnityEngine;

public class GameplayAssetProvider : MonoBehaviour
{
    private static GameplayAssetProvider _instance;
    public GameObject emptyBlock;
    public GameObject floorBlock;
    public GameObject wallBlock;
    public GameObject trapBlock;
    public GameObject obstacleBlock;
    public GameObject goalBlock;

    public static GameplayAssetProvider Instance
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

        _instance = Resources.Load<GameplayAssetProvider>(Strings.AssetProvidersPath + "GameplayAssetProvider");
        DontDestroyOnLoad(_instance);
    }


    public static GameObject GetBlock(BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Empty => Instance.emptyBlock,
            BlockType.Floor => Instance.floorBlock,
            BlockType.Wall => Instance.wallBlock,
            BlockType.Trap => Instance.trapBlock,
            BlockType.Obstacle => Instance.obstacleBlock,
            BlockType.Goal => Instance.goalBlock,
            _ => throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null)
        };
    } 
}