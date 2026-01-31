using System;
using UnityEngine;
using DG.Tweening;
using Object = UnityEngine.Object;

public class GridSpawner
{
    private LevelSO level;
    private Camera cam;
    private Transform rightGridRoot;
    private Transform leftGridRoot;
    private float gridPlaneY = 0f;

    // Layout
    private const float PADDING_WORLD = 0.1f;
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

        SpawnFinishTile();

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

        var center = ViewportToPlanePoint(new Vector2(0.5f, 0.6f), gridPlaneY);

        var leftDef = level.GetPlayerDefinition(PlayerSide.Left);
        var rightDef = level.GetPlayerDefinition(PlayerSide.Right);

        // Distance from grid root to its outermost perimeter wall/gate center
        float leftHalfW = (leftDef.gridSize.x + 1) * leftDef.cellSize * 0.5f;
        float rightHalfW = (rightDef.gridSize.x + 1) * rightDef.cellSize * 0.5f;

        // 1 tile gap between the inner gates
        float gap = leftDef.cellSize;

        leftGridRoot.position = new Vector3(center.x - leftHalfW - gap * 0.5f, center.y, center.z);
        rightGridRoot.position = new Vector3(center.x + rightHalfW + gap * 0.5f, center.y, center.z);
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

        // Full width including perimeter walls + 1 tile gap between grids
        float leftFullW = (leftDef.gridSize.x + 2) * leftDef.cellSize;
        float rightFullW = (rightDef.gridSize.x + 2) * rightDef.cellSize;
        float leftH = leftDef.gridSize.y * leftDef.cellSize;
        float rightH = rightDef.gridSize.y * rightDef.cellSize;
        float gap = leftDef.cellSize;
        float totalW = leftFullW + gap + rightFullW + PADDING_WORLD * 2f;
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

        var left  = ViewportToPlanePoint(new Vector2(0.0f, 0.6f), y);
        var right = ViewportToPlanePoint(new Vector2(1f, 0.6f), y);
        planeWidth = Vector3.Distance(left, right);

        var bottom = ViewportToPlanePoint(new Vector2(0.6f, 0f), y);
        var top    = ViewportToPlanePoint(new Vector2(0.6f, 1f), y);
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

        SpawnPerimeterWalls(parent, def, halfExtents, side);

        return grid;
    }

    private void SpawnPerimeterWalls(Transform parent, PlayerGridDefinition def, Vector2 halfExtents, PlayerSide side)
    {
        int w = def.gridSize.x;
        int h = def.gridSize.y;
        float cell = def.cellSize;
        float wallHeight = 1f;

        var topRot = Quaternion.Euler(0f, 0f, 0f);
        var bottomRot = Quaternion.Euler(0f, 180f, 0f);
        var leftRot = Quaternion.Euler(0f, -90f, 0f);
        var rightRot = Quaternion.Euler(0f, 90f, 0f);

        // Start gate: one cell outside the grid behind the start cell
        var start = def.playerStartCell;
        Vector2Int startGate;
        Quaternion startGateRot;

        if (start.y == 0)          { startGate = new Vector2Int(start.x, -1);  startGateRot = bottomRot; }
        else if (start.y == h - 1) { startGate = new Vector2Int(start.x, h);   startGateRot = topRot; }
        else if (start.x == 0)     { startGate = new Vector2Int(-1, start.y);   startGateRot = leftRot; }
        else                        { startGate = new Vector2Int(w, start.y);    startGateRot = rightRot; }

        // Goal gate: one cell outside the grid behind the goal cell, facing towards screen middle
        var goal = def.targetCell;
        // Goal gate always on the inner edge (facing towards screen middle)
        Quaternion goalGateRot = side == PlayerSide.Left ? rightRot : leftRot;
        Vector2Int goalGate = side == PlayerSide.Left
            ? new Vector2Int(w, goal.y)
            : new Vector2Int(-1, goal.y);

        // Bottom and top edges (no corners)
        for (int x = 0; x < w; x++)
        {
            bool isStartGate = startGate.x == x && startGate.y == -1;
            bool isGoalGate = goalGate.x == x && goalGate.y == -1;
            if (!isStartGate && !isGoalGate)
                SpawnWall(parent, x, -1, cell, halfExtents, wallHeight, bottomRot);

            isStartGate = startGate.x == x && startGate.y == h;
            isGoalGate = goalGate.x == x && goalGate.y == h;
            if (!isStartGate && !isGoalGate)
                SpawnWall(parent, x, h, cell, halfExtents, wallHeight, topRot);
        }

        // Left and right edges (no corners)
        for (int y = 0; y < h; y++)
        {
            bool isStartGate = startGate.x == -1 && startGate.y == y;
            bool isGoalGate = goalGate.x == -1 && goalGate.y == y;
            if (!isStartGate && !isGoalGate)
                SpawnWall(parent, -1, y, cell, halfExtents, wallHeight, leftRot);

            isStartGate = startGate.x == w && startGate.y == y;
            isGoalGate = goalGate.x == w && goalGate.y == y;
            if (!isStartGate && !isGoalGate)
                SpawnWall(parent, w, y, cell, halfExtents, wallHeight, rightRot);
        }

        // Spawn gates
        SpawnGate(parent, startGate.x, startGate.y, cell, halfExtents, startGateRot);
        SpawnGate(parent, goalGate.x, goalGate.y, cell, halfExtents, goalGateRot);

        // Wall below the goal gate
        SpawnWall(parent, goalGate.x, goalGate.y - 1, cell, halfExtents, wallHeight, bottomRot);
    }

    private void SpawnFinishTile()
    {
        if (level.finishPrefab == null) return;

        var leftDef = level.GetPlayerDefinition(PlayerSide.Left);
        var rightDef = level.GetPlayerDefinition(PlayerSide.Right);

        // Goal gate positions in world space
        int leftW = leftDef.gridSize.x;
        var leftHalf = new Vector2((leftW - 1) * leftDef.cellSize * 0.5f, (leftDef.gridSize.y - 1) * leftDef.cellSize * 0.5f);
        var leftGateWorld = leftGridRoot.position + new Vector3(
            leftW * leftDef.cellSize - leftHalf.x,
            0f,
            leftDef.targetCell.y * leftDef.cellSize - leftHalf.y
        );

        int rightW = rightDef.gridSize.x;
        var rightHalf = new Vector2((rightW - 1) * rightDef.cellSize * 0.5f, (rightDef.gridSize.y - 1) * rightDef.cellSize * 0.5f);
        var rightGateWorld = rightGridRoot.position + new Vector3(
            -1 * rightDef.cellSize - rightHalf.x,
            0f,
            rightDef.targetCell.y * rightDef.cellSize - rightHalf.y
        );

        // Finish tile at the midpoint between the two goal gates
        var midpoint = (leftGateWorld + rightGateWorld) * 0.5f;
        var finishPos = midpoint + new Vector3(1f, 0f, leftDef.cellSize);

        var finish = Object.Instantiate(level.finishPrefab, finishPos, Quaternion.identity);
        finish.transform.parent = null;

        // Walls around the finish tile (2 cells wide, so offset by cellSize from center)
        float cell = leftDef.cellSize;
        float wallHeight = 1f;

        // Top wall
        var topWall = Object.Instantiate(level.wallPrefab, finishPos + new Vector3(0f, 0f, cell), Quaternion.Euler(0f, 0f, 0f));
        topWall.tag = "Wall";
        topWall.AddComponent<BoxCollider>().size = new Vector3(cell, wallHeight, cell);
        
        // Top wall
        var topLeftWall = Object.Instantiate(level.wallPrefab, finishPos + new Vector3(-cell, 0f, cell), Quaternion.Euler(0f, 0f, 0f));
        topLeftWall.tag = "Wall";
        topLeftWall.AddComponent<BoxCollider>().size = new Vector3(cell, wallHeight, cell);

        // Left wall
        var leftWall = Object.Instantiate(level.wallPrefab, finishPos + new Vector3(-cell *2, 0f, 0f), Quaternion.Euler(0f, -90f, 0f));
        leftWall.tag = "Wall";
        leftWall.AddComponent<BoxCollider>().size = new Vector3(cell, wallHeight, cell);

        // Right wall
        var rightWall = Object.Instantiate(level.wallPrefab, finishPos + new Vector3(cell, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
        rightWall.tag = "Wall";
        rightWall.AddComponent<BoxCollider>().size = new Vector3(cell, wallHeight, cell);
    }

    private void SpawnGate(Transform parent, int x, int y, float cellSize, Vector2 halfExtents, Quaternion rotation)
    {
        if (level.gatePrefab == null) return;

        var worldPos = parent.position + new Vector3(
            x * cellSize - halfExtents.x,
            0f,
            y * cellSize - halfExtents.y
        );

        var floor = Object.Instantiate(GameplayAssetProvider.GetBlock(BlockType.Floor), worldPos, Quaternion.identity);
        floor.transform.parent = parent;

        var gate = Object.Instantiate(level.gatePrefab, worldPos, rotation);
        gate.transform.parent = parent;
        gate.tag = "Wall";
    }

    private void SpawnWall(Transform parent, int x, int y, float cellSize, Vector2 halfExtents, float wallHeight, Quaternion? rotation = null)
    {
        var worldPos = parent.position + new Vector3(
            x * cellSize - halfExtents.x,
            0f,
            y * cellSize - halfExtents.y
        );

        var rot = rotation ?? Quaternion.identity;
        var wall = Object.Instantiate(level.wallPrefab, worldPos, rot);
        wall.transform.parent = parent;
        wall.tag = "Wall";

        var col = wall.AddComponent<BoxCollider>();
        col.size = new Vector3(cellSize, wallHeight, cellSize);
        col.center = new Vector3(0f, wallHeight * 0.5f, 0f);
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
