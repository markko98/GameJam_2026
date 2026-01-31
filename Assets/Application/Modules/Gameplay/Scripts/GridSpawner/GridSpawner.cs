using System;
using UnityEngine;
using DG.Tweening;

public class GridSpawner
{
    private LevelSO level;
    private Camera cam;
    private Transform rightGridRoot;
    private Transform leftGridRoot;
    private float gridPlaneY = 0f;

    // Layout
    private const float DEFAULT_GAP_WORLD = 1.0f;
    private const float PADDING_WORLD = 0.75f;
    private const float MIN_ORTHO = 1f;
    private const float MAX_ORTHO = 200f;

    // Animation
    private const float POP_TIME = 0.12f;

    public void Initialize(Transform rightGridRoot, Transform leftGridRoot, LevelSO levelData)
    {
        this.rightGridRoot = rightGridRoot;
        this.leftGridRoot = leftGridRoot;
        this.level = levelData;

        cam = Camera.main;
        if (!cam) Debug.LogWarning("GridSpawner: Camera.main not found. Grid placement may be incorrect.");

        // 1) Fit camera first (so viewport placement is correct)
        FitCameraToTwoGrids();

        // 2) Place roots in viewport halves (tilt-safe)
        PositionGridRootsToViewportHalves();
    }

    /// <summary>
    /// Instantiates ALL blocks immediately, then animates their scale pop with per-block delays.
    /// Both grids run in parallel. onAllFinished called after both complete.
    /// </summary>
    public void SpawnBothGridsAnimated(Action<PlayerSide, BlockView[,]> onSpawned, Action onAllFinished)
    {
        if (level == null )
        {
            Debug.LogError("GridSpawner: Missing level or outlet.");
            return;
        }

        // Spawn both instantly
        var leftGrid = SpawnGrid(PlayerSide.Left);
        var rightGrid = SpawnGrid(PlayerSide.Right);

        onSpawned?.Invoke(PlayerSide.Left, leftGrid);
        onSpawned?.Invoke(PlayerSide.Right, rightGrid);

        // Animate both in parallel and wait for both to finish via counter
        int pending = 2;

        AnimateGridSpawn(leftGrid, level.spawnAnimDelayPerBlock, () =>
        {
            pending--;
            if (pending <= 0) onAllFinished?.Invoke();
        });

        AnimateGridSpawn(rightGrid, level.spawnAnimDelayPerBlock, () =>
        {
            pending--;
            if (pending <= 0) onAllFinished?.Invoke();
        });
    }

    private void PositionGridRootsToViewportHalves()
    {
        cam = Camera.main;
        if (!cam)
        {
            Debug.LogError("GridSpawner: Camera.main missing.");
            return;
        }
        
        leftGridRoot.position  = ViewportToPlanePoint(new Vector2(0.25f, 0.5f), gridPlaneY);
        rightGridRoot.position = ViewportToPlanePoint(new Vector2(0.75f, 0.5f), gridPlaneY);
    }

    private Vector3 ViewportToPlanePoint(Vector2 viewport01, float planeY)
    {
        var ray = cam.ViewportPointToRay(new Vector3(viewport01.x, viewport01.y, 0f));
        var plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        if (!plane.Raycast(ray, out float enter))
            return Vector3.zero;

        return ray.GetPoint(enter);
    }

    private void FitCameraToTwoGrids()
    {
        cam = Camera.main;
        if (!cam || !cam.orthographic)
        {
            Debug.LogWarning("GridSpawner: FitCameraToTwoGrids requires an orthographic camera.");
            return;
        }

        var leftDef  = level.GetPlayerDefinition(PlayerSide.Left);
        var rightDef = level.GetPlayerDefinition(PlayerSide.Right);

        float leftW  = leftDef.gridSize.x * leftDef.cellSize;
        float leftH  = leftDef.gridSize.y * leftDef.cellSize;

        float rightW = rightDef.gridSize.x * rightDef.cellSize;
        float rightH = rightDef.gridSize.y * rightDef.cellSize;

        float totalW = leftW + DEFAULT_GAP_WORLD + rightW + PADDING_WORLD * 2f;
        float maxH   = Mathf.Max(leftH, rightH) + PADDING_WORLD * 2f;

        float lo = MIN_ORTHO;
        float hi = MAX_ORTHO;

        for (int i = 0; i < 30; i++)
        {
            float mid = (lo + hi) * 0.5f;
            cam.orthographicSize = mid;

            GetPlaneVisibleSizeAtCenter(out float planeW, out float planeH);

            bool fits = planeW >= totalW && planeH >= maxH;
            if (fits) hi = mid;
            else lo = mid;
        }

        cam.orthographicSize = hi;
    }

    private void GetPlaneVisibleSizeAtCenter(out float planeWidth, out float planeHeight)
    {
        float y = gridPlaneY;

        var left  = ViewportToPlanePoint(new Vector2(0f, 0.5f), y);
        var right = ViewportToPlanePoint(new Vector2(1f, 0.5f), y);
        planeWidth = Vector3.Distance(left, right);

        var bottom = ViewportToPlanePoint(new Vector2(0.5f, 0f), y);
        var top    = ViewportToPlanePoint(new Vector2(0.5f, 1f), y);
        planeHeight = Vector3.Distance(bottom, top);
    }

    private BlockView[,] SpawnGrid(PlayerSide side)
    {
        var def = level.GetPlayerDefinition(side);
        if (def == null)
        {
            Debug.LogError($"GridSpawner: Missing player definition for {side}.");
            return null;
        }

        var parent = (side == PlayerSide.Left) ? leftGridRoot : rightGridRoot;
        if (parent == null)
        {
            Debug.LogError($"GridSpawner: Missing grid root for {side} on GameplayOutlet.");
            return null;
        }

        int w = def.gridSize.x;
        int h = def.gridSize.y;

        var grid = new BlockView[w, h];

        // center around parent
        var halfExtents = new Vector2((w - 1) * def.cellSize * 0.5f, (h - 1) * def.cellSize * 0.5f);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var cell = def.GetCell(x, y);

            var prefab = GameplayAssetProvider.GetBlock(cell);
            if (!prefab)
            {
                Debug.LogError($"GridSpawner: No prefab for blockType={cell} at {side} ({x},{y}).");
                continue;
            }

            var worldPos = parent.position + new Vector3(
                x * def.cellSize - halfExtents.x,
                0f,
                y * def.cellSize - halfExtents.y
            );

            var go = UnityEngine.Object.Instantiate(prefab, worldPos, Quaternion.identity, parent);

            var view = go.GetComponent<BlockView>();
            if (!view) view = go.AddComponent<BlockView>();

            view.Initialize(side, cell);

            // start hidden
            view.transform.localScale = Vector3.zero;

            grid[x, y] = view;
        }

        return grid;
    }

    /// <summary>
    /// Animates all blocks immediately via tweens. Delay offsets start times, no waiting between.
    /// Calls onFinished after the last tween completes.
    /// </summary>
    private void AnimateGridSpawn(BlockView[,] grid, float delayPerBlock, Action onFinished)
    {
        if (grid == null)
        {
            onFinished?.Invoke();
            return;
        }

        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        // Optional: order by distance to center for nicer wave
        Vector2 center = new Vector2((w - 1) * 0.5f, (h - 1) * 0.5f);

        // We'll track max end time and fire completion after it.
        float maxEndTime = 0f;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var view = grid[x, y];
            if (!view) continue;

            // Delay: either simple index based, or “wave from center”
            // Simple index order:
            // int order = y * w + x;

            // Wave order (nicer):
            float dist = Vector2.Distance(new Vector2(x, y), center);
            int order = Mathf.RoundToInt(dist * 10f); // quantize so it’s not too “random”

            float delay = delayPerBlock * order;

            // scale pop with ease
            view.transform
                .DOScale(Vector3.one, POP_TIME)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic);

            float endTime = delay + POP_TIME;
            if (endTime > maxEndTime) maxEndTime = endTime;
        }

        // Fire after the last one finishes (no coroutine)
        DOVirtual.DelayedCall(maxEndTime, () => onFinished?.Invoke());
    }
}
