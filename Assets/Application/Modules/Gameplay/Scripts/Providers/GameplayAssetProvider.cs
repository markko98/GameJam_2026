using System;
using UnityEngine;

public class GameplayAssetProvider : MonoBehaviour
{
    private static GameplayAssetProvider _instance;
    public GameObject emptyBlock;
    public GameObject floorBlock;
    public GameObject floor1Block;
    public GameObject trapBlock;
    public GameObject obstacleBlock;
    public GameObject goalBlock;
    public GameObject startBlock;
    public GameObject bridgeBlock;
    public GameObject arrowDispenserBlock;
    public GameObject natureBridgeBlock;


    
    public GameObject player1;
    public GameObject player2;
    
    public Material level1Skybox;
    public Material level2Skybox;
    public Material level3Skybox;
    public Material level4Skybox;

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
            BlockType.Floor1 => Instance.floor1Block,
            BlockType.Trap => Instance.trapBlock,
            BlockType.Obstacle => Instance.obstacleBlock,
            BlockType.Goal => Instance.goalBlock,
            BlockType.Start => Instance.startBlock,
            BlockType.Bridge => Instance.bridgeBlock,
            BlockType.ArrowDispenser => Instance.arrowDispenserBlock,
            BlockType.NatureBridge => Instance.natureBridgeBlock,
            _ => throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null)
        };
    }

    public static GameObject GetPlayer(PlayerType playerType)
    {
        return playerType switch
        {
            PlayerType.Player1 => Instance.player1,
            PlayerType.Player2 => Instance.player2,
            _ => throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null)
        };
    }

    public static Material GetSkybox(LevelType levelType)
    {
        return levelType switch
        {
            LevelType.Level1 => Instance.level1Skybox,
            LevelType.Level2 => Instance.level2Skybox,
            _ => Instance.level1Skybox
        };
    }
}