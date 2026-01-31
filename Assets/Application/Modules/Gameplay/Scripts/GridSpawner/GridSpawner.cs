using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GridSpawner
{
    // on initialize spawn grid for each player defined in scriptable object for this level (can be any size - 3x3, 5x8, 9x9...)
    // each player has different level
    // animate grid spawn then spawn player that looks cool and fun

    private GameplayOutlet outlet;
    private LevelSO level;
    private Camera cam;

    // Call this after Initialize(outlet, level) AND after you decide camera size (optional).
    private void PositionGridRootsToViewportHalves()
    {
        cam = Camera.main;
        if (!cam)
        {
            Debug.LogError("GridSpawner: Camera.main missing.");
            return;
        }

        // Center of left half & right half in viewport
        outlet.leftGridRoot.position  = ViewportToPlanePoint(new Vector2(0.25f, 0.5f), outlet.gridPlaneY);
        outlet.rightGridRoot.position = ViewportToPlanePoint(new Vector2(0.75f, 0.5f), outlet.gridPlaneY);
    }

    private Vector3 ViewportToPlanePoint(Vector2 viewport01, float planeY)
    {
        var ray = cam.ViewportPointToRay(new Vector3(viewport01.x, viewport01.y, 0f));
        var plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        if (!plane.Raycast(ray, out float enter))
            return Vector3.zero;

        return ray.GetPoint(enter);
    }

    // Tweak these
    private const float DEFAULT_GAP_WORLD = 1.0f;   // gap between left and right grid in world units
    private const float PADDING_WORLD = 0.75f;      // padding around content in world units
    private const float MIN_ORTHO = 1f;
    private const float MAX_ORTHO = 200f;

    private void FitCameraToTwoGrids()
    {
        cam = Camera.main;
        if (!cam || !cam.orthographic)
        {
            Debug.LogWarning("GridSpawner: FitCameraToTwoGrids requires an orthographic camera.");
            return;
        }

        // Compute required content size in world units (on the plane)
        var leftDef  = level.GetPlayerDefinition(PlayerSide.Left);
        var rightDef = level.GetPlayerDefinition(PlayerSide.Right);

        float leftW  = leftDef.gridSize.x * leftDef.cellSize;
        float leftH  = leftDef.gridSize.y * leftDef.cellSize;

        float rightW = rightDef.gridSize.x * rightDef.cellSize;
        float rightH = rightDef.gridSize.y * rightDef.cellSize;

        float totalW = leftW + DEFAULT_GAP_WORLD + rightW + PADDING_WORLD * 2f;
        float maxH   = Mathf.Max(leftH, rightH) + PADDING_WORLD * 2f;

        // Binary search ortho size until plane-visible width/height can contain required content
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

    /// <summary>
    /// Returns approximate visible size on the ground plane using center cross-sections.
    /// This is stable for tilted ortho cameras.
    /// </summary>
    private void GetPlaneVisibleSizeAtCenter(out float planeWidth, out float planeHeight)
    {
        float y = outlet.gridPlaneY;

        // width at viewport y = 0.5 (center horizontal slice)
        var left  = ViewportToPlanePoint(new Vector2(0f, 0.5f), y);
        var right = ViewportToPlanePoint(new Vector2(1f, 0.5f), y);
        planeWidth = Vector3.Distance(left, right);

        // height at viewport x = 0.5 (center vertical slice)
        var bottom = ViewportToPlanePoint(new Vector2(0.5f, 0f), y);
        var top    = ViewportToPlanePoint(new Vector2(0.5f, 1f), y);
        planeHeight = Vector3.Distance(bottom, top);
    }

    public void Initialize(GameplayOutlet outlet, LevelSO levelData)
    {
        this.outlet = outlet;
        this.level = levelData;

        if (!Camera.main)
            Debug.LogWarning("GridSpawner: Camera.main not found. Grid placement may be incorrect.");

        PositionGridRootsToScreenHalves();
        FitCameraToTwoGrids();
        PositionGridRootsToViewportHalves();
    }

    public IEnumerator SpawnBothGridsAnimated(Action<PlayerSide, BlockView[,]> onSpawned, Action onAllFinished)
    {
        // Left player
        var left = SpawnGrid(PlayerSide.Left);
        yield return AnimateGridSpawn(left, level.spawnAnimDelayPerBlock);
        onSpawned?.Invoke(PlayerSide.Left, left);

        // Right player
        var right = SpawnGrid(PlayerSide.Right);
        yield return AnimateGridSpawn(right, level.spawnAnimDelayPerBlock);
        onSpawned?.Invoke(PlayerSide.Right, right);

        onAllFinished?.Invoke();
    }

    private BlockView[,] SpawnGrid(PlayerSide side)
    {
        var def = level.GetPlayerDefinition(side);
        if (def == null)
        {
            Debug.LogError($"GridSpawner: Missing player definition for {side}.");
            return null;
        }

        var parent = (side == PlayerSide.Left) ? outlet.leftGridRoot : outlet.rightGridRoot;
        if (parent == null)
        {
            Debug.LogError($"GridSpawner: Missing grid root for {side} on GameplayOutlet.");
            return null;
        }

        int w = def.gridSize.x;
        int h = def.gridSize.y;

        var grid = new BlockView[w, h];

        // Center grid inside its half by offsetting start position
        var halfExtents = new Vector2((w - 1) * def.cellSize * 0.5f, (h - 1) * def.cellSize * 0.5f);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var cell = def.GetCell(x, y);
            if (cell == null)
            {
                Debug.LogError($"GridSpawner: Cell is null at {side} ({x},{y}).");
                continue;
            }

            var prefab = GameplayAssetProvider.GetBlock(cell.blockType);
            if (!prefab)
            {
                Debug.LogError($"GridSpawner: No prefab for blockType={cell.blockType} at {side} ({x},{y}).");
                continue;
            }

            var worldPos = parent.position + new Vector3(
                x * def.cellSize - halfExtents.x,
                0f,
                y * def.cellSize - halfExtents.y
            );

            var go = UnityEngine.Object.Instantiate(prefab, worldPos, Quaternion.identity, parent);
            var view = go.GetComponent<BlockView>();
            if (!view)
            {
                view = go.AddComponent<BlockView>();
            }

            view.Initialize(side, new Vector2Int(x, y), cell.blockType, cell.startState);

            // Start hidden for animation
            view.transform.localScale = Vector3.zero;

            grid[x, y] = view;
        }

        return grid;
    }

    private IEnumerator AnimateGridSpawn(BlockView[,] grid, float delayPerBlock)
    {
        if (grid == null) yield break;

        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        // simple “pop” animation using scale lerp
        const float popTime = 0.12f;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var view = grid[x, y];
            if (!view) continue;

            yield return ScalePop(view.transform, popTime);

            if (delayPerBlock > 0f)
                yield return new WaitForSeconds(delayPerBlock);
        }
    }

    private IEnumerator ScalePop(Transform t, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / time);
            // ease out
            k = 1f - Mathf.Pow(1f - k, 3f);
            t.localScale = Vector3.one * k;
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private void PositionGridRootsToScreenHalves()
    {
        var cam = Camera.main;
        if (!cam || !cam.orthographic)
        {
            // fallback: just keep outlet roots as they are
            return;
        }

        // World extents of camera (orthographic)
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // Put left root at center of left half, right root at center of right half
        // z placement uses outlet.gridPlaneZ (or existing z)
        var z = 0;
        outlet.leftGridRoot.position  = new Vector3(-halfW * 0.5f, 0, z);
        outlet.rightGridRoot.position = new Vector3( halfW * 0.5f, 0, z);
    }
}
