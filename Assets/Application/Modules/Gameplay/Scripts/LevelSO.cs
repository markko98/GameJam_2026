using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/LevelSO", fileName = "LevelSO")]
public class LevelSO : ScriptableObject
{
    // grid of Block
    // definition of level - defining level design - starting player pos - block types and their starting states
    // mask types available in a level
    // target block location

    [Header("Grid Spawn Animation")]
    [Range(0f, 0.2f)]
    public float spawnAnimDelayPerBlock = 0.01f;

    [Header("Players")]
    public PlayerGridDefinition leftPlayer;
    public PlayerGridDefinition rightPlayer;

    public GameObject wallPrefab;
    public GameObject gatePrefab;
    public GameObject finishPrefab;

    public PlayerGridDefinition GetPlayerDefinition(PlayerSide side)
    {
        return side == PlayerSide.Left ? leftPlayer : rightPlayer;
    }
}



