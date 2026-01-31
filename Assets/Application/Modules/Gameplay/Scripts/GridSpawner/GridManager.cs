using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    // keeps cache of the spawned grid
    // calls spawner to spawn
    // listens to mask change and update all blocks in level to its target state
    // check where is the player when the mask changes - if death condition raise an event for PlayerDeathTrigger

    private readonly LevelSO levelData;
    private const float posY = 0.25f;


    private Transform rightGridRoot;
    private Transform leftGridRoot;
    
    public GridSpawner gridSpawner;

    private readonly Dictionary<PlayerSide, BlockView[,]> spawnedGrids = new();
    
    private EventBinding<MaskTriggeredEvent> maskTriggeredEvent;
    private EventBinding<MaskExpiredEvent> maskExpiredEvent;


    public GridManager(LevelSO levelData, Transform rightGridRoot, Transform leftGridRoot)
    {
        this.levelData = levelData;

        this.rightGridRoot = rightGridRoot;
        this.leftGridRoot = leftGridRoot;
        gridSpawner = new GridSpawner();

        maskTriggeredEvent = new EventBinding<MaskTriggeredEvent>(OnMaskChanged);
        UEventBus<MaskTriggeredEvent>.Register(maskTriggeredEvent);

        maskExpiredEvent = new EventBinding<MaskExpiredEvent>((e) =>
        {
            OnMaskChanged(new MaskTriggeredEvent() { maskType = e.maskType });
        });
        
        UEventBus<MaskExpiredEvent>.Register(maskExpiredEvent);
    }

    public void SpawnAndAnimateGrid()
    {
        if (levelData == null)
        {
            Debug.LogError("GridManager: levelData is null.");
            return;
        }

        gridSpawner.Initialize(rightGridRoot, leftGridRoot, levelData);

        // Spawn both grids and animate
        gridSpawner.SpawnBothGridsAnimated(
            onSpawned: (side, grid) =>
            {
                spawnedGrids[side] = grid;
            },
            onAllFinished: () =>
            {
                // Optional: spawn players after grid animation (if you have such method)
                // outlet.playerSpawner.SpawnPlayers(levelData);
                // For now, just log:
                Debug.Log("Grids spawned & animated. Ready to spawn players.");
                SpawnPlayers();
            }
        );
    }

    private void SpawnPlayers()
    {
        var startBlockLeft = GetPlayerStartBlock(PlayerSide.Left);
        var startBlockRight = GetPlayerStartBlock(PlayerSide.Right);

        var leftPlayerPrefab = GameplayAssetProvider.GetPlayer(PlayerType.Player1);
        var rightPlayerPrefab = GameplayAssetProvider.GetPlayer(PlayerType.Player2);

        var leftPlayer = UnityEngine.Object.Instantiate(leftPlayerPrefab, startBlockLeft.transform.position, Quaternion.identity, null);
        var rightPlayer = UnityEngine.Object.Instantiate(rightPlayerPrefab, startBlockRight.transform.position, Quaternion.identity, null);

        var startPosLeft = startBlockLeft.transform.position;
        startPosLeft.y = posY;
        
        var startPosRight = startBlockRight.transform.position;
        startPosRight.y = posY;
        
        leftPlayer.transform.position = startPosLeft;
        rightPlayer.transform.position = startPosRight;
    }

    /// <summary>
    /// Call this when a mask changes (shared mask list for both players, or per-player – you decide).
    /// This will update all blocks according to each block’s target state for that mask.
    /// </summary>
    public void OnMaskChanged(MaskTriggeredEvent e)
    {
        foreach (var kvp in spawnedGrids)
        {
            var side = kvp.Key;
            var grid = kvp.Value;

            var def = levelData.GetPlayerDefinition(side);
            if (def == null) continue;

            int w = def.gridSize.x;
            int h = def.gridSize.y;

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var view = grid[x, y];
                if (view == null) continue;

                view.ChangeVisuals(e.maskType);
            }
        }
    }

    public BlockView[,] GetSpawnedGrid(PlayerSide side)
    {
        return spawnedGrids.TryGetValue(side, out var grid) ? grid : null;
    }

    public BlockView GetPlayerStartBlock(PlayerSide side)
    {
        var grid = GetSpawnedGrid(side);
        if (grid == null) return null;

        var def = levelData.GetPlayerDefinition(side);
        if (def == null) return null;

        Vector2Int start = def.playerStartCell;

        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        if (start.x < 0 || start.y < 0 || start.x >= w || start.y >= h)
        {
            Debug.LogError($"Start cell out of bounds for {side}: {start} (grid {w}x{h})");
            return null;
        }

        return grid[start.x, start.y];
    }


    public void CleanUp()
    {
        UEventBus<MaskTriggeredEvent>.Deregister(maskTriggeredEvent);
        UEventBus<MaskExpiredEvent>.Deregister(maskExpiredEvent);
    }
}
