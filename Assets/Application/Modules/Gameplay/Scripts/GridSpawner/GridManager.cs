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

    private Transform rightGridRoot;
    private Transform leftGridRoot;
    
    public GridSpawner gridSpawner;

    // Cache: per player -> spawned block views [x,y]
    private readonly Dictionary<PlayerSide, BlockView[,]> spawnedGrids = new();

    // Hooks you can connect from outside:
    // - Provide where player currently is (grid cell) so we can detect death on mask-change
    public Func<PlayerSide, Vector2Int> GetPlayerCell;

    // - Trigger player death (or forward to your own event bus)
    public Action<PlayerSide> OnPlayerDeath;
    
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

        maskExpiredEvent = new EventBinding<MaskExpiredEvent>(() =>
        {
            OnMaskChanged(new MaskTriggeredEvent() { maskType = MaskType.None });
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
            }
        );
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
                var cell = def.GetCell(x, y);
                var view = grid[x, y];
                if (view == null) continue;

                // Determine target state for this mask (fallback to startState)
                var newState = cell.GetStateForMask(e.maskType, cell.startState);
                view.ChangeVisuals(e.maskType);
                view.ApplyState(newState);
            }

            // Death check after updating blocks
            CheckDeathCondition(side);
        }
    }

    private void CheckDeathCondition(PlayerSide side)
    {
        if (GetPlayerCell == null) return;

        var playerCell = GetPlayerCell.Invoke(side);

        var def = levelData.GetPlayerDefinition(side);
        if (def == null) return;

        if (!def.IsInside(playerCell))
            return;

        var cell = def.GetCell(playerCell.x, playerCell.y);
        if (cell == null) return;

        if (cell.startState == BlockState.Deadly)
        {
            OnPlayerDeath?.Invoke(side);
        }
    }

    public BlockView[,] GetSpawnedGrid(PlayerSide side)
    {
        return spawnedGrids.TryGetValue(side, out var grid) ? grid : null;
    }

    public void CleanUp()
    {
        UEventBus<MaskTriggeredEvent>.Deregister(maskTriggeredEvent);
        UEventBus<MaskExpiredEvent>.Deregister(maskExpiredEvent);
    }
}
